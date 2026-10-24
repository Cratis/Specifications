// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;

if (args.Length != 2)
{
    throw new ArgumentException("Supply one probe assembly name and its plain assertion type.");
}
if (AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
    assembly.GetName().Name!.StartsWith("Probe.", StringComparison.Ordinal) ||
    assembly.GetName().Name!.StartsWith("Cratis.Specifications.XUnit", StringComparison.Ordinal)))
{
    throw new InvalidOperationException("Assertion registration must not be preloaded by the host.");
}

var probe = Assembly.Load(new AssemblyName(args[0]));
var registration = probe.GetTypes().Single(type => type.Name == "CratisSpecificationsAssertionFailures");
var register = registration.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)!;
if (!register.IsDefined(typeof(ModuleInitializerAttribute), false))
{
    throw new InvalidOperationException("The selected assembly must retain its generated module initializer.");
}

try
{
    probe.GetType(args[1], throwOnError: true)!.GetMethod("Fail")!.Invoke(null, null);
    throw new InvalidOperationException("The plain assertion did not fail.");
}
catch (TargetInvocationException exception) when (exception.InnerException?.GetType().FullName == "Xunit.Sdk.XunitException")
{
    Console.WriteLine($"Native pre-lifecycle xUnit assertion from {probe.GetName().Name}.");
}

var loadedProbes = AppDomain.CurrentDomain.GetAssemblies()
    .Where(assembly => assembly.GetName().Name!.StartsWith("Probe.", StringComparison.Ordinal)).ToArray();
if (loadedProbes.Length != 1 || loadedProbes[0] != probe)
{
    throw new InvalidOperationException("Another probe assembly could mask missing assertion registration.");
}
