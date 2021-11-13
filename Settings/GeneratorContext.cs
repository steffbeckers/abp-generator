namespace SteffBeckers.Abp.Generator.Settings
{
    public class GeneratorContext
    {
        public string AggregateRootName { get; set; } = "Test";

        // TODO: Update with getter based on AggregateRootName
        public string AggregateRootNamePlural { get; set; } = "Tests";

        public string ProjectName { get; set; } = "MyCompany.MyProduct";
    }
}