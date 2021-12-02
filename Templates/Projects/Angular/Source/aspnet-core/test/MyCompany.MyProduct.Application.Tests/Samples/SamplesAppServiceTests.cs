using MyCompany.MyProduct.Shared;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyCompany.MyProduct.Samples
{
    public class SamplesAppServiceTests : MyProductApplicationTestBase
    {
        private readonly ISampleRepository _sampleRepository;
        private readonly ISamplesAppService _samplesAppService;

        public SamplesAppServiceTests()
        {
            _sampleRepository = GetRequiredService<ISampleRepository>();
            _samplesAppService = GetRequiredService<ISamplesAppService>();
        }

        [Fact]
        public async Task Should_Create_Sample()
        {
            // Arrange
            SampleCreateInputDto input = new SampleCreateInputDto()
            {
                Name = "Sample name",
                Description = "Sample description"
            };

            // Act
            SampleDto sampleDto = await _samplesAppService.CreateAsync(input);

            // Assert
            Sample sample = await _sampleRepository.FindAsync(x => x.Id == sampleDto.Id);

            sample.ShouldNotBe(null);
            sample.Name.ShouldBe("Sample name");
            sample.Description.ShouldBe("Sample description");
        }

        [Fact]
        public async Task Should_Delete_Sample()
        {
            // Arrange
            Guid sampleId = Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17");

            // Act
            await _samplesAppService.DeleteAsync(sampleId);

            // Assert
            Sample sample = await _sampleRepository.FindAsync(sampleId);

            sample.ShouldBe(null);
        }

        [Fact]
        public async Task Should_Get_List_Of_Samples()
        {
            // Arrange
            SampleListInputDto input = new SampleListInputDto();

            // Act
            PagedListDto<SampleListDto> listDto = await _samplesAppService.GetListAsync(input);

            // Assert
            listDto.TotalCount.ShouldBe(2);
            listDto.Items.Count.ShouldBe(2);
            listDto.Items.Any(x => x.Id == Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17")).ShouldBe(true);
            listDto.Items.Any(x => x.Id == Guid.Parse("2084de4b-3e9c-4179-8269-674c97cbef28")).ShouldBe(true);

            SampleListDto sampleDto1 = listDto.Items.FirstOrDefault(x => x.Id == Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17"));
            sampleDto1.ShouldNotBe(null);
            sampleDto1.Name.ShouldBe("Sample 1 name");
            sampleDto1.Description.ShouldBe("Sample 1 description");
        }

        [Fact]
        public async Task Should_Get_Lookup_List_Of_Samples()
        {
            // Arrange
            LookupInputDto input = new LookupInputDto()
            {
                FilterText = "Sample 1"
            };

            // Act
            PagedListDto<LookupDto<Guid>> lookupDto = await _samplesAppService.GetLookupAsync(input);

            // Assert
            lookupDto.TotalCount.ShouldBe(1);
            lookupDto.Items.Count.ShouldBe(1);

            LookupDto<Guid> sampleDto1 = lookupDto.Items.FirstOrDefault(x => x.Id == Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17"));
            sampleDto1.ShouldNotBe(null);
            sampleDto1.Name.ShouldBe("Sample 1 name");
        }

        [Fact]
        public async Task Should_Get_Lookup_List_Of_Samples_Based_On_Id()
        {
            // Arrange
            LookupInputDto input = new LookupInputDto()
            {
                Id = Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17")
            };

            // Act
            PagedListDto<LookupDto<Guid>> lookupDto = await _samplesAppService.GetLookupAsync(input);

            // Assert
            lookupDto.TotalCount.ShouldBe(1);
            lookupDto.Items.Count.ShouldBe(1);

            LookupDto<Guid> sampleDto1 = lookupDto.Items.FirstOrDefault(x => x.Id == Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17"));
            sampleDto1.ShouldNotBe(null);
            sampleDto1.Name.ShouldBe("Sample 1 name");
        }

        [Fact]
        public async Task Should_Get_Sample()
        {
            // Arrange
            Guid sampleId = Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17");

            // Act
            SampleDto sampleDto = await _samplesAppService.GetAsync(sampleId);

            // Assert
            sampleDto.ShouldNotBe(null);
            sampleDto.Id.ShouldBe(sampleId);
            sampleDto.Name.ShouldBe("Sample 1 name");
            sampleDto.Description.ShouldBe("Sample 1 description");
        }

        [Fact]
        public async Task Should_Update_Sample()
        {
            // Arrange
            Guid sampleId = Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17");
            SampleUpdateInputDto input = new SampleUpdateInputDto()
            {
                Name = "Sample name update",
                Description = "Sample description update"
            };

            // Act
            SampleDto sampleDto = await _samplesAppService.UpdateAsync(sampleId, input);

            // Assert
            Sample sample = await _sampleRepository.FindAsync(sampleDto.Id);

            sample.ShouldNotBe(null);
            sample.Name.ShouldBe("Sample name update");
            sample.Description.ShouldBe("Sample description update");
        }
    }
}