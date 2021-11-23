param ([string] $version, [string] $apiKey)

dotnet pack -c Release
dotnet nuget push bin/Release/SteffBeckers.Abp.Generator.$version.nupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json
