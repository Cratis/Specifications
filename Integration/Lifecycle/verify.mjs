// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdirSync, mkdtempSync, readFileSync } from 'node:fs';
import { resolve, join } from 'node:path';

export const projects = {
    XUnit: 'XUnit/Consumer.XUnit.csproj',
    XUnitV3: 'XUnit.v3/XUnit.v3.csproj',
    NUnit4: 'NUnit/Consumer.NUnit.csproj',
    NUnit5: 'NUnit5/NUnit5.csproj',
    TUnit: 'TUnit/Consumer.TUnit.csproj',
    Runner: 'Runner/Runner.csproj',
};

const adapter = process.argv[2];
assert.ok(Object.hasOwn(projects, adapter), `Unknown adapter: ${adapter}`);
const output = resolve(process.argv[3] ?? 'artifacts/lifecycle');
mkdirSync(output, { recursive: true });
const directory = mkdtempSync(join(output, `${adapter}-`));
const trace = join(directory, 'trace.txt');
const project = `Integration/Consumers/${projects[adapter]}`;
const mtp = adapter === 'TUnit' || adapter === 'Runner';
const args = mtp
    ? ['run', '--project', project, '-c', 'Release', '--no-build', '--', '--report-trx', '--report-trx-filename', 'result.trx', '--results-directory', directory]
    : ['test', project, '-c', 'Release', '--no-build', '--logger', 'trx;LogFileName=result.trx', '--results-directory', directory];
if (adapter === 'Runner') args.push('--specs-cache', 'none', '--treenode-filter', '/*/LifecycleProbes/*/*');
const run = spawnSync('dotnet', args, {
    encoding: 'utf8', timeout: 90_000,
    env: { ...process.env, CRATIS_LIFECYCLE_TRACE: trace },
});
process.stdout.write(run.stdout ?? '');
process.stderr.write(run.stderr ?? '');
assert.ifError(run.error);
assert.equal(run.signal, null, 'The probe must exit without being killed');
assert.equal(run.status, mtp ? 2 : 1, 'Expected native test-failure exit code');
const xml = readFileSync(join(directory, 'result.trx'), 'utf8');
const calls = readFileSync(trace, 'utf8').trim().split(/\r?\n/);
const cases = ['setup', 'act', 'cleanup'].flatMap(phase =>
    ['failure', 'cancellation'].flatMap(kind => [`${phase}_${kind}`, `async_${phase}_${kind}`]));
const attributes = text => Object.fromEntries([...text.matchAll(/([\w:]+)="([^"]*)"/g)].map(match => [match[1], match[2]]));
const definitions = new Map([...xml.matchAll(/<UnitTest\b([^>]*)>([\s\S]*?)<\/UnitTest>/g)].map(match => {
    const method = match[2].match(/<TestMethod\b([^>]*)/);
    return [attributes(match[1]).id, method ? attributes(method[1]).className : ''];
}));
const results = [...xml.matchAll(/<UnitTestResult\b([^>]*)/g)].map(match => attributes(match[1]));
assert.equal(results.length, adapter === 'Runner' ? 28 : 24, 'Every expected fact/teardown must have a terminal result');
assert.equal(new Set(results.map(result => result.executionId)).size, results.length, 'No duplicate terminal results');
for (const name of cases) {
    const operations = calls.filter(call => call.startsWith(`${name}:`)).map(call => call.slice(name.length + 1));
    assert.equal(operations.filter(call => call === 'Establish').length, 1, `${name}: setup once`);
    assert.equal(operations.filter(call => call === 'Because').length, name.includes('setup') ? 0 : 1, `${name}: act eligibility`);
    assert.equal(operations.filter(call => call === 'Destroy').length, 1, `${name}: cleanup once`);
    const facts = operations.filter(call => call.startsWith('fact')).sort();
    assert.deepEqual(facts, name.includes('cleanup') ? ['fact1', 'fact2'] : [], `${name}: fact eligibility`);
    const factResults = results.filter(result => definitions.get(result.testId)?.split(',')[0] === `LifecycleProbes.${name}`);
    const ordinary = factResults.filter(result => !result.testName.includes('teardown'));
    assert.equal(ordinary.length, 2, `${name}: both facts discovered through ${adapter}`);
    if (!name.includes('cleanup')) {
        assert.ok(ordinary.every(result => result.outcome === 'Failed'), `${name}: both facts failed`);
    } else {
        const expected = adapter === 'TUnit' ? ['Failed', 'Passed'] : ['Passed', 'Passed'];
        assert.deepEqual(ordinary.map(result => result.outcome).sort(), expected, `${name}: preserve framework-owned cleanup reporting`);
        if (adapter === 'Runner') {
            const teardown = factResults.filter(result => result.testName.includes('teardown'));
            assert.equal(teardown.length, 1, `${name}: separate native teardown result`);
            assert.equal(teardown[0].outcome, 'Failed', `${name}: teardown fails the run`);
        } else if (adapter.startsWith('XUnit')) {
            assert.ok(`${run.stdout}${run.stderr}`.includes(`Test Class Cleanup Failure (LifecycleProbes.${name})`), `${name}: xUnit class cleanup diagnostic`);
        } else if (adapter.startsWith('NUnit')) {
            assert.ok(`${run.stdout}${run.stderr}`.includes(`TearDown failed for test fixture LifecycleProbes.${name}`), `${name}: NUnit fixture cleanup diagnostic`);
        }
    }
}
for (const phase of ['Establish', 'Because', 'Destroy']) {
    assert.ok(xml.includes(`${phase}() was cancelled`), `${phase}: framework must report lifecycle cancellation`);
}
for (const phase of ['setup', 'act', 'cleanup']) {
    assert.ok(xml.includes(`deliberate ${phase} failure`), `${phase}: original failure must be reported`);
}
console.log(`${adapter}: 12 shared lifecycle cases, ${results.length} terminal results, once-only cleanup verified (${directory})`);
