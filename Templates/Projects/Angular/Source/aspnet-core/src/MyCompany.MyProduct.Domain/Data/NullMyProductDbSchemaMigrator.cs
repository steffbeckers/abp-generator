using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace MyCompany.MyProduct.Data
{
    /* This is used if database provider does't define
     * IMyProductDbSchemaMigrator implementation.
     */
    public class NullMyProductDbSchemaMigrator : IMyProductDbSchemaMigrator, ITransientDependency
    {
        public Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }
}