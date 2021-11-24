Param ([Parameter(Mandatory=$true)][string] $apiKey)

# Increment version
$projectPath = "SteffBeckers.Abp.Generator.csproj";
[xml] $csproj = Get-Content $projectPath;

$version = $csproj.Project.PropertyGroup.Version.Split(".");
$major = [int]$version[0];
$minor = [int]$version[1];
$patch = [int]$version[2];

$patch = $patch + 1;

$csproj.Project.PropertyGroup.Version = "$major.$minor.$patch";
$csproj.Save($projectPath);

dotnet pack -c Release
dotnet nuget push bin/Release/SteffBeckers.Abp.Generator.$major.$minor.$patch.nupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json

git add SteffBeckers.Abp.Generator.csproj
git commit -m "Published version $major.$minor.$patch"
git push
