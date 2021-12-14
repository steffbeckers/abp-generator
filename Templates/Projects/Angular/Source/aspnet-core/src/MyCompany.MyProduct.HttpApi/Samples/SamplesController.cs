using Microsoft.AspNetCore.Mvc;
using MyCompany.MyProduct.Controllers;
using MyCompany.MyProduct.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace MyCompany.MyProduct.Samples
{
    [RemoteService]
    [Area("app")]
    [ControllerName("Samples")]
    [Route("api/app/samples")]
    public class SamplesController : MyProductController, ISamplesAppService
    {
        private readonly ISamplesAppService _samplesAppService;

        public SamplesController(ISamplesAppService samplesAppService)
        {
            _samplesAppService = samplesAppService;
        }

        /// <summary>
        /// Create a sample.
        /// </summary>
        /// <param name="input"><see cref="SampleCreateInputDto"/>.</param>
        [HttpPost]
        public Task<SampleDto> CreateAsync(SampleCreateInputDto input)
        {
            return _samplesAppService.CreateAsync(input);
        }

        /// <summary>
        /// Delete a sample.
        /// </summary>
        /// <param name="id">Sample id.</param>
        [HttpDelete]
        [Route("{id}")]
        public Task DeleteAsync(Guid id)
        {
            return _samplesAppService.DeleteAsync(id);
        }

        /// <summary>
        /// Get a sample.
        /// </summary>
        /// <param name="id">Sample id.</param>
        [HttpGet]
        [Route("{id}")]
        public Task<SampleDto> GetAsync(Guid id)
        {
            return _samplesAppService.GetAsync(id);
        }

        /// <summary>
        /// Get a list of samples.
        /// </summary>
        /// <param name="input"><see cref="SampleListInputDto"/>.</param>
        [HttpGet]
        public Task<PagedListDto<SampleListDto>> GetListAsync(SampleListInputDto input)
        {
            return _samplesAppService.GetListAsync(input);
        }

        /// <summary>
        /// Get a lookup list of samples.
        /// </summary>
        /// <param name="input"><see cref="LookupInputDto"/>.</param>
        [HttpGet]
        [Route("lookup")]
        public Task<PagedListDto<LookupDto<Guid>>> GetLookupAsync(LookupInputDto input)
        {
            return _samplesAppService.GetLookupAsync(input);
        }

        /// <summary>
        /// Update a sample.
        /// </summary>
        /// <param name="id">Sample id.</param>
        /// <param name="input"><see cref="SampleUpdateInputDto"/>.</param>
        [HttpPut]
        [Route("{id}")]
        public Task<SampleDto> UpdateAsync(Guid id, SampleUpdateInputDto input)
        {
            return _samplesAppService.UpdateAsync(id, input);
        }
    }
}
