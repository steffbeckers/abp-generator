using MyCompany.MyProduct.EntityFrameworkCore;
using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace MyCompany.MyProduct.Samples
{
    public class EfCoreSampleRepository : EfCoreRepository<MyProductDbContext, Sample, Guid>, ISampleRepository
    {
        public EfCoreSampleRepository(IDbContextProvider<MyProductDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}