# Code generator for ABP.io projects


## Usage

```powershell
sbabpgen
```

## dotnet tool NuGet package

![Nuget](https://img.shields.io/nuget/v/SteffBeckers.Abp.Generator?style=for-the-badge)

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

OR

```powershell
publish.ps1 <API key here>
```
