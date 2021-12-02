using MyCompany.MyProduct.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace MyCompany.MyProduct.Samples
{
    public interface ISamplesAppService : IApplicationService
    {
        Task<SampleDto> CreateAsync(SampleCreateInputDto input);

        Task DeleteAsync(Guid id);

        Task<SampleDto> GetAsync(Guid id);

        Task<SampleDto> UpdateAsync(Guid id, SampleUpdateInputDto input);

        Task<PagedListDto<SampleListDto>> GetListAsync(SampleListInputDto input);

        Task<PagedListDto<LookupDto<Guid>>> GetLookupAsync(LookupInputDto input);
    }
}