using AutoMapper;
using MyCompany.MyProduct.Samples;
using MyCompany.MyProduct.Shared;
using System;

namespace MyCompany.MyProduct
{
    public class MyProductApplicationAutoMapperProfile : Profile
    {
        public MyProductApplicationAutoMapperProfile()
        {
            // You can configure your AutoMapper mapping configuration here.
            // Alternatively, you can split your mapping configurations
            // into multiple profile classes for a better organization.

            CreateMap<Sample, LookupDto<Guid>>();
            CreateMap<Sample, SampleDto>();
            CreateMap<Sample, SampleListDto>();
            CreateMap<Sample, SampleSimpleDto>();
        }
    }
}