using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace MyCompany.MyProduct
{
    public class MyProductTestDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        public Task SeedAsync(DataSeedContext context)
        {
            /* Seed additional test data... */

            return Task.CompletedTask;
        }
    }
}