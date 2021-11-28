using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCompany.MyProduct.Data;
using Volo.Abp.DependencyInjection;

namespace MyCompany.MyProduct.EntityFrameworkCore
{
    public class EntityFrameworkCoreMyProductDbSchemaMigrator
        : IMyProductDbSchemaMigrator, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityFrameworkCoreMyProductDbSchemaMigrator(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task MigrateAsync()
        {
            /* We intentionally resolving the MyProductDbContext
             * from IServiceProvider (instead of directly injecting it)
             * to properly get the connection string of the current tenant in the
             * current scope.
             */

            await _serviceProvider
                .GetRequiredService<MyProductDbContext>()
                .Database
                .MigrateAsync();
        }
    }
}
