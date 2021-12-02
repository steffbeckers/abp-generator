using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyCompany.MyProduct.Permissions;
using MyCompany.MyProduct.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.ObjectMapping;

namespace MyCompany.MyProduct.Samples
{
    [RemoteService(IsEnabled = false)]
    [Authorize(MyProductPermissions.Samples.Default)]
    public class SamplesAppService : MyProductAppService, ISamplesAppService
    {
        private readonly ISampleRepository _sampleRepository;

        public SamplesAppService(ISampleRepository sampleRepository)
        {
            _sampleRepository = sampleRepository;
        }

        [Authorize(MyProductPermissions.Samples.Create)]
        public async Task<SampleDto> CreateAsync(SampleCreateInputDto input)
        {
            Sample sample = new Sample(
                GuidGenerator.Create(),
                input.Name)
            {
                Description = input.Description
            };

            sample = await _sampleRepository.InsertAsync(sample);

            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(sample.Id);
        }

        [Authorize(MyProductPermissions.Samples.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _sampleRepository.DeleteAsync(id);
        }

        public async Task<SampleDto> GetAsync(Guid id)
        {
            IQueryable<Sample> sampleQueryable = await _sampleRepository.GetQueryableAsync();

            SampleDto sampleDto = await ObjectMapper.GetMapper()
                .ProjectTo<SampleDto>(sampleQueryable.AsSingleQuery())
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sampleDto == null)
            {
                throw new BusinessException(MyProductDomainErrorCodes.Samples.NotFound)
                    .WithData("Id", id);
            }

            return sampleDto;
        }

        public async Task<PagedListDto<SampleListDto>> GetListAsync(SampleListInputDto input)
        {
            IQueryable<Sample> sampleQueryable = await _sampleRepository.GetQueryableAsync();

            sampleQueryable = sampleQueryable.WhereIf(
                !string.IsNullOrWhiteSpace(input.FilterText),
                x => x.Name.Contains(input.FilterText) ||
                    x.Description.Contains(input.FilterText));

            long totalCount = await sampleQueryable.LongCountAsync();

            if (string.IsNullOrEmpty(input.Sorting))
            {
                sampleQueryable = sampleQueryable.OrderBy(x => x.Name);
            }
            else
            {
                sampleQueryable = sampleQueryable.OrderBy(x => input.Sorting);
            }

            sampleQueryable = sampleQueryable.PageBy(input.SkipCount, input.MaxResultCount);

            return new PagedListDto<SampleListDto>()
            {
                TotalCount = totalCount,
                Items = await ObjectMapper.GetMapper()
                    .ProjectTo<SampleListDto>(sampleQueryable)
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        public async Task<PagedListDto<LookupDto<Guid>>> GetLookupAsync(LookupInputDto input)
        {
            IQueryable<Sample> sampleQueryable = await _sampleRepository.GetQueryableAsync();

            sampleQueryable = sampleQueryable
                .WhereIf(
                    !string.IsNullOrWhiteSpace(input.FilterText),
                    x => x.Name.Contains(input.FilterText))
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id);

            long totalCount = await sampleQueryable.LongCountAsync();

            sampleQueryable = sampleQueryable.OrderBy(x => x.Name)
                .PageBy(input.SkipCount, input.MaxResultCount);

            return new PagedListDto<LookupDto<Guid>>()
            {
                TotalCount = totalCount,
                Items = await ObjectMapper.GetMapper()
                    .ProjectTo<LookupDto<Guid>>(sampleQueryable)
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        [Authorize(MyProductPermissions.Samples.Update)]
        public async Task<SampleDto> UpdateAsync(Guid id, SampleUpdateInputDto input)
        {
            Sample sample = await _sampleRepository.GetAsync(id);

            sample.Name = input.Name;
            sample.Description = input.Description;

            sample = await _sampleRepository.UpdateAsync(sample);

            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(sample.Id);
        }
    }
}