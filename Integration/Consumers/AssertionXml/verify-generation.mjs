// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { readFileSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const [project, assemblyName, previousWriteTime] = process.argv.slice(2);
assert.ok(['First', 'Second'].includes(project), 'Select the First or Second probe project.');
assert.ok(assemblyName, 'An expected assembly name is required.');
const root = fileURLToPath(new URL(`./${project}/`, import.meta.url));
const sourcePath = `${root}obj/Release/net10.0/CratisSpecificationsAssertionFailures.g.cs`;
const source = readFileSync(sourcePath, 'utf8');
const encodedName = Buffer.from(assemblyName).toString('base64')
    .replaceAll('+', '_p').replaceAll('/', '_s').replaceAll('=', '_e');
const registrationType = `Cratis.Generated.A${encodedName}.CratisSpecificationsAssertionFailures`;
assert.match(encodedName, /^[A-Za-z0-9_]+$/, 'The encoded name must be valid in a C# identifier.');
assert.ok(source.includes(`namespace Cratis.Generated.A${encodedName};`), 'Generated namespace must match AssemblyName.');
assert.ok(source.includes('[global::System.Runtime.CompilerServices.ModuleInitializer]'), 'Automatic registration must remain.');
assert.ok(source.includes('internal static void Register() => global::Cratis.Specifications.AssertionFailureRegistration.Register();'),
    'The generated initializer must call the native assertion registration.');
const xml = readFileSync(`${root}bin/Release/net10.0/${assemblyName}.xml`, 'utf8');
assert.ok(xml.includes(`name="M:${registrationType}.Register"`), 'XML must document the assembly-specific registration.');
assert.ok(xml.includes('Registers the XUnit assertion failure factory'), 'Registration documentation must remain.');
if (previousWriteTime !== undefined) {
    assert.equal(String(statSync(sourcePath).mtimeMs), previousWriteTime, 'An unchanged build must not rewrite the source.');
}
console.log(`Verified generated source and XML for ${assemblyName}.`);
