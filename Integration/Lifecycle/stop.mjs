// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { randomUUID } from 'node:crypto';
import { mkdirSync, mkdtempSync, existsSync, readFileSync, writeFileSync } from 'node:fs';
import { createServer } from 'node:net';
import { once } from 'node:events';
import { join, resolve } from 'node:path';
import { setTimeout as delay } from 'node:timers/promises';
import { connect } from './rpc.mjs';

const output = resolve(process.argv[2] ?? 'artifacts/lifecycle');
mkdirSync(output, { recursive: true });
const directory = mkdtempSync(join(output, 'Stop-'));
const trace = join(directory, 'trace.txt');
const server = createServer();
server.listen(0, '127.0.0.1');
await once(server, 'listening');
const connection = once(server, 'connection');
const child = spawn('dotnet', [
    'Integration/Consumers/Runner/bin/Release/net10.0/Runner.dll',
    '--server', '--client-port', String(server.address().port),
    '--specs-parallel', '2', '--specs-cache', 'none',
], { env: { ...process.env, CRATIS_LIFECYCLE_TRACE: trace }, stdio: ['ignore', 'pipe', 'pipe'] });
let stdout = '';
let stderr = '';
child.stdout.on('data', chunk => { stdout += chunk; });
child.stderr.on('data', chunk => { stderr += chunk; });
const exit = once(child, 'exit');
let socket;
const watchdog = setTimeout(() => child.kill('SIGKILL'), 30_000);
const bounded = promise => Promise.race([promise, delay(20_000, null, { ref: false }).then(() => { throw new Error('MTP protocol operation timed out'); })]);
const lines = () => existsSync(trace) ? readFileSync(trace, 'utf8').trim().split(/\r?\n/) : [];
async function until(predicate) {
    const deadline = Date.now() + 20_000;
    while (!predicate()) {
        assert.equal(child.exitCode, null, `Host exited before the barrier: ${stderr}`);
        assert.ok(Date.now() < deadline, `Timed out waiting for probe barrier: ${lines().join(', ')}`);
        await delay(10);
    }
}
try {
    [socket] = await bounded(connection);
    const rpc = connect(socket);
    const initialized = await bounded(rpc.request('initialize', {
        processId: process.pid,
        clientInfo: { name: 'Cratis lifecycle proof', version: '1.0.0' },
        capabilities: { testing: { debuggerProvider: false, isStateful: false } },
        protocolVersions: ['1.0.0'],
    }).response);
    assert.equal(initialized.error, undefined, 'MTP initialization must succeed');
    const filter = '/*/StopProbes/*/*';
    const discovered = await bounded(rpc.request('testing/discoverTests', { runId: randomUUID(), filter }).response);
    assert.equal(discovered.error, undefined, 'MTP discovery must succeed');
    const discoveryUpdates = rpc.updates.splice(0);
    const selected = discoveryUpdates.filter(update => update.method === 'testing/testUpdates/tests').flatMap(update => update.params.changes ?? []);
    assert.equal(selected.length, 6, 'Real MTP discovery must deliver all six selected facts before Stop');
    const run = rpc.request('testing/runTests', { runId: randomUUID(), filter });
    await until(() => ['A_first', 'B_second'].every(name => lines().includes(`${name}:Started`)));
    assert.ok(!lines().some(line => line.startsWith('Z_queued:')), 'Third class must still be queued');
    rpc.notify('$/cancelRequest', { id: run.id });
    await until(() => ['A_first', 'B_second'].every(name => lines().includes(`${name}:Dispose`)));
    const response = await bounded(run.response);
    writeFileSync(join(directory, 'messages.json'), JSON.stringify({ discoveryUpdates, response, updates: rpc.updates }, null, 2));
    assert.equal(response.error?.code, -32800, 'MTP must acknowledge the request as cancelled');
    const changes = rpc.updates.filter(update => update.method === 'testing/testUpdates/tests').flatMap(update => update.params.changes ?? []);
    assert.equal(changes.length, 0, 'MTP 2.5.1 hard cancellation suppresses subsequent result updates, including queued facts');
    rpc.notify('exit', null);
    const [code, signal] = await bounded(exit);
    assert.equal(signal, null, 'MTP host must exit normally after Stop');
    assert.equal(code, 0, 'Server-mode exit after a cancelled request');
    for (const name of ['A_first', 'B_second']) {
        for (const operation of ['Create', 'Started', 'Cancelled', 'Destroy', 'Dispose']) {
            assert.equal(lines().filter(line => line === `${name}:${operation}`).length, 1, `${name}: ${operation} exactly once`);
        }
        assert.ok(!lines().some(line => line.startsWith(`${name}:fact`)), `${name}: cancelled act must not run facts`);
    }
    assert.ok(!lines().some(line => line.startsWith('Z_queued:')), 'Cancelled scheduling must not construct queued class');
    console.log(`MTP hard Stop: two concurrent classes cancelled; Destroy/Dispose once each; queued class not constructed; six facts discovered, zero result updates after hard cancellation; exit ${code}. Messages: ${directory}`);
} finally {
    clearTimeout(watchdog);
    socket?.destroy();
    server.close();
    if (child.exitCode === null) child.kill('SIGKILL');
    writeFileSync(join(directory, 'stdout.txt'), stdout);
    writeFileSync(join(directory, 'stderr.txt'), stderr);
}
