using System;
using Volo.Abp.Domain.Repositories;

namespace MyCompany.MyProduct.Samples
{
    public interface ISampleRepository : IRepository<Sample, Guid>
    {
    }
}