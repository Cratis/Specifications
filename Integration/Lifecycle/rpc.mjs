// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';

// MTP server-mode JSON-RPC uses UTF-8 Content-Length framing, not newline-delimited JSON.
export function connect(socket) {
    let buffer = Buffer.alloc(0);
    let sequence = 0;
    const pending = new Map();
    const updates = [];
    const send = message => {
        const body = Buffer.from(JSON.stringify({ jsonrpc: '2.0', ...message }));
        socket.write(Buffer.concat([Buffer.from(`Content-Length: ${body.length}\r\n\r\n`), body]));
    };
    const dispatch = message => {
        if (message.method) {
            updates.push(message);
            if (message.id !== undefined) send({ id: message.id, result: null });
            return;
        }
        assert.ok(pending.has(message.id), `Unexpected RPC response ${message.id}`);
        pending.get(message.id)(message);
        pending.delete(message.id);
    };
    socket.on('data', chunk => {
        buffer = Buffer.concat([buffer, chunk]);
        while (true) {
            const end = buffer.indexOf('\r\n\r\n');
            if (end < 0) return;
            const length = Number(buffer.subarray(0, end).toString().match(/Content-Length:\s*(\d+)/i)?.[1]);
            assert.ok(Number.isSafeInteger(length) && length > 0, 'Valid MTP frame required');
            if (buffer.length < end + 4 + length) return;
            const message = JSON.parse(buffer.subarray(end + 4, end + 4 + length).toString());
            buffer = buffer.subarray(end + 4 + length);
            dispatch(message);
        }
    });
    return {
        updates,
        notify: (method, params) => send({ method, params }),
        request(method, params) {
            const id = ++sequence;
            const response = new Promise(resolve => pending.set(id, resolve));
            send({ id, method, params });
            return { id, response };
        },
    };
}
