using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyCompany.MyProduct.Data.Seeding.Contributors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Uow;

namespace MyCompany.MyProduct.Data.Seeding
{
    public abstract class DataSeederBase : IDataSeeder
    {
        private readonly AbpDataSeedOptions _dataSeedOptions;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public DataSeederBase(
            IOptions<AbpDataSeedOptions> dataSeedOptions,
            IServiceScopeFactory serviceScopeFactory,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _dataSeedOptions = dataSeedOptions.Value;
            _serviceScopeFactory = serviceScopeFactory;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public virtual DataSeedContributorList GetContributors()
        {
            // Add our own data seed contributors here in correct order
            return new DataSeedContributorList { typeof(IdentityServerDataSeedContributor) };
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            // Filter to only get the framework's data seed contributors
            IEnumerable<Type> dataSeedContributors = _dataSeedOptions.Contributors
                .Where(x => !x.FullName.StartsWith("MyCompany.MyProduct.Data.Seeding"));

            // Append our own data seed contributors
            dataSeedContributors = dataSeedContributors.Concat(GetContributors());

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                foreach (Type contributorType in dataSeedContributors)
                {
                    IDataSeedContributor contributor = (IDataSeedContributor)scope
                        .ServiceProvider
                        .GetRequiredService(contributorType);

                    await contributor.SeedAsync(context);

                    await _unitOfWorkManager.Current.SaveChangesAsync();
                }
            }
        }
    }
}
