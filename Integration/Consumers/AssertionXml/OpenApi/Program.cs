// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Xml.Linq;
using Consumer.AssertionXml;
using Consumer.AssertionXml.First;
using Consumer.AssertionXml.Second;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var members = new Dictionary<string, XElement>();
foreach (var assembly in new[] { typeof(FirstProbe).Assembly, typeof(SecondProbe).Assembly })
{
    var xmlDocument = XDocument.Load(Path.ChangeExtension(assembly.Location, ".xml"));
    var registration = xmlDocument.Descendants("member").Single(member =>
        member.Attribute("name")!.Value.EndsWith("CratisSpecificationsAssertionFailures.Register", StringComparison.Ordinal));
    if (string.IsNullOrWhiteSpace(registration.Element("summary")?.Value))
    {
        throw new InvalidOperationException("Registration documentation must be retained.");
    }
    foreach (var member in xmlDocument.Descendants("member"))
    {
        members.Add(member.Attribute("name")!.Value, member);
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddOpenApi();
await using var app = builder.Build();
app.MapGet("/probe", () => "documented assemblies loaded");
app.MapGet("/first", () => new FirstResponse("first"));
app.MapGet("/second", () => new SecondResponse("second"));
app.MapOpenApi();
await app.StartAsync();
var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
using var client = new HttpClient { BaseAddress = new Uri(addresses.Addresses.Single()) };
var json = await client.GetStringAsync("/openapi/v1.json");
using var document = JsonDocument.Parse(json);
var paths = document.RootElement.GetProperty("paths");
foreach (var path in new[] { "/probe", "/first", "/second" })
{
    if (!paths.TryGetProperty(path, out _))
    {
        throw new InvalidOperationException($"OpenAPI did not describe {path}.");
    }
}
var descriptions = document.RootElement.GetProperty("components").GetProperty("schemas")
    .EnumerateObject().Select(schema => schema.Value.TryGetProperty("description", out var description) ? description.GetString() : null)
    .ToArray();
foreach (var expected in new[] { "First assembly response documentation.", "Second assembly response documentation." })
{
    if (!descriptions.Contains(expected))
    {
        throw new InvalidOperationException($"OpenAPI did not preserve XML description: {expected}");
    }
}
await app.StopAsync();
Console.WriteLine($"Merged {members.Count} documented XML members and generated the OpenAPI endpoint.");
