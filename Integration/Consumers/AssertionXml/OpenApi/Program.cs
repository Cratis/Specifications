// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Linq;
using Consumer.AssertionXml;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var members = new Dictionary<string, XElement>();
foreach (var assembly in new[] { typeof(FirstProbe).Assembly, typeof(SecondProbe).Assembly })
{
    var document = XDocument.Load(Path.ChangeExtension(assembly.Location, ".xml"));
    var registration = document.Descendants("member").Single(member =>
        member.Attribute("name")!.Value.EndsWith("CratisSpecificationsAssertionFailures.Register", StringComparison.Ordinal));
    if (string.IsNullOrWhiteSpace(registration.Element("summary")?.Value))
    {
        throw new InvalidOperationException("Registration documentation must be retained.");
    }
    foreach (var member in document.Descendants("member"))
    {
        members.Add(member.Attribute("name")!.Value, member);
    }
}

foreach (var fail in new Action[] { FirstProbe.Fail, SecondProbe.Fail })
{
    try
    {
        fail();
        throw new InvalidOperationException("The assertion did not fail.");
    }
    catch (Exception exception) when (exception.GetType().FullName == "Xunit.Sdk.XunitException")
    {
        Console.WriteLine("Native xUnit assertion: " + fail.Method.DeclaringType!.Assembly.GetName().Name);
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddOpenApi();
await using var app = builder.Build();
app.MapGet("/probe", () => "documented assemblies loaded");
app.MapOpenApi();
await app.StartAsync();
var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
using var client = new HttpClient { BaseAddress = new Uri(addresses.Addresses.Single()) };
var json = await client.GetStringAsync("/openapi/v1.json");
if (!json.Contains("/probe", StringComparison.Ordinal))
{
    throw new InvalidOperationException("OpenAPI did not describe the probe endpoint.");
}
await app.StopAsync();
Console.WriteLine($"Merged {members.Count} documented XML members and generated the OpenAPI endpoint.");
