using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyCompany.MyProduct.Data.Seeding.Contributors;
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

            // Add our own unit test data seed contributors here in correct order. Example:
            contributors.Add(typeof(SamplesUnitTestDataSeedContributor));

            return contributors;
        }
    }
}