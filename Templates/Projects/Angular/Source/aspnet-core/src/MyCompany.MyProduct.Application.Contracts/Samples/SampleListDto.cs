using System;
using Volo.Abp.Application.Dtos;

namespace MyCompany.MyProduct.Samples
{
    public class SampleListDto : EntityDto<Guid>
    {
        public string Description { get; set; }

        public string Name { get; set; }
    }
}