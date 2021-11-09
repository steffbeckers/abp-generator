# Code generator for ABP.io projects

## NuGet

https://www.nuget.org/packages/SteffBeckers.Abp.Generator

### Installation

```powershell
dotnet tool install -g SteffBeckers.Abp.Generator
```

### Updates

```powershell
dotnet tool update -g SteffBeckers.Abp.Generator
```

### Release

```powershell
dotnet pack -c Release
```

```powershell
dotnet nuget push SteffBeckers.Abp.Generator.x.x.x.nupkg --api-key <API key here> --source https://api.nuget.org/v3/index.json
```
