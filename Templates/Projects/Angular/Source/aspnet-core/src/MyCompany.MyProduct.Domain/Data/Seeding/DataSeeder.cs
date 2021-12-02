using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace MyCompany.MyProduct.Data.Seeding
{
    [Dependency(ReplaceServices = true)]
    public class DataSeeder : DataSeederBase, ITransientDependency
    {
        public DataSeeder(
            IOptions<AbpDataSeedOptions> dataSeedOptions,
            IServiceScopeFactory serviceScopeFactory,
            IUnitOfWorkManager unitOfWorkManager)
            : base(dataSeedOptions, serviceScopeFactory, unitOfWorkManager)
        {
        }

        public override DataSeedContributorList GetContributors()
        {
            DataSeedContributorList contributors = base.GetContributors();

            // Only seed test data in Development or Test environment
            string environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if ((environmentName == "Development") || (environmentName == "Test"))
            {
                // Add our own test data seed contributors here in correct order
                // contributors.Add(typeof(SamplesTestDataSeedContributor));
            }

            return contributors;
        }
    }
}