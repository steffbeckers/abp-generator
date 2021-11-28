using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Reflection;

namespace SteffBeckers.Abp.Generator.Updates;

    public class UpdateService
    {
        private readonly SourceCacheContext _cache = new SourceCacheContext();
        private readonly NuGet.Common.ILogger _logger = NullLogger.Instance;
        private readonly SourceRepository _repository = Repository.Factory
            .GetCoreV3("https://api.nuget.org/v3/index.json");

        public readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        public async Task CheckForUpdateAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Version: {Version}");

            PackageMetadataResource resource = await _repository.GetResourceAsync<PackageMetadataResource>();

            IEnumerable<IPackageSearchMetadata> packages = await resource.GetMetadataAsync(
                "SteffBeckers.Abp.Generator",
                includePrerelease: false,
                includeUnlisted: false,
                _cache,
                _logger,
                cancellationToken);

            IPackageSearchMetadata? latestPackage = packages.OrderByDescending(x => x.Identity.Version).FirstOrDefault();

            if (latestPackage == null)
            {
                return;
            }

            string latestVersion = latestPackage.Identity.Version.Version.ToString(3);

            if (Version == latestVersion)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"New update available! Use following command to update:");
            Console.WriteLine();
            Console.WriteLine($"dotnet tool update -g SteffBeckers.Abp.Generator --no-cache");
            Console.WriteLine();
        }
    }
