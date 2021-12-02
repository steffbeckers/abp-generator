using System.Threading.Tasks;

namespace MyCompany.MyProduct.Data
{
    public interface IMyProductDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}