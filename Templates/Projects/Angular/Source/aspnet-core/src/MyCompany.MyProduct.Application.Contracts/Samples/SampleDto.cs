using System;
using Volo.Abp.Application.Dtos;

namespace MyCompany.MyProduct.Samples
{
    public class SampleDto : FullAuditedEntityDto<Guid>
    {
        public string Description { get; set; }

        public string Name { get; set; }
    }
}