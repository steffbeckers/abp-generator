using MyCompany.MyProduct.Samples;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace MyCompany.MyProduct.Data.Seeding.Contributors
{
    public class SamplesUnitTestDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly ISampleRepository _sampleRepository;

        public SamplesUnitTestDataSeedContributor(ISampleRepository sampleRepository)
        {
            _sampleRepository = sampleRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            Sample sample1 = new Sample(
                id: Guid.Parse("8a39344b-832a-4169-8105-a649e311cc17"),
                name: "Sample 1 name")
            {
                Description = "Sample 1 description"
            };

            await _sampleRepository.InsertAsync(sample1);

            Sample sample2 = new Sample(
                id: Guid.Parse("2084de4b-3e9c-4179-8269-674c97cbef28"),
                name: "Sample 2 name")
            {
                Description = "Sample 2 description"
            };

            await _sampleRepository.InsertAsync(sample2);
        }
    }
}