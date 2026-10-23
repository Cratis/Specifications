---
applyTo: "**/*"
---

## Commands

```bash
dotnet build Specifications.sln --configuration Release
```

`dotnet test` on the whole solution does not work: the TUnit and native runner specs run on
Microsoft.Testing.Platform, which cannot be mixed with the VSTest based xUnit and NUnit projects. Run each spec
project directly, exactly as `.github/workflows/build.yml` does:

```bash
dotnet test Source/<Project>/<Project>.csproj --configuration Release --no-build   # CodeAnalysis.Specs, XUnit.Specs, XUnit.DefaultBehavior.Specs, XUnit.v3.Specs, XUnit.v3.DefaultBehavior.Specs, NUnit.Specs, NUnit5.Specs
dotnet run --project Source/TUnit.Specs/TUnit.Specs.csproj --configuration Release --no-build
dotnet run --project Source/Runner.Specs/Runner.Specs.csproj --configuration Release --no-build
```

The Sample projects intentionally contain failing specs and are not part of the test run.
