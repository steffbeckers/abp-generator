namespace SteffBeckers.Abp.Generator.Settings;

public class GeneratorSettings
{
    public GeneratorContext Context { get; set; } = new GeneratorContext();

    public string ProjectPath { get; set; } = string.Empty;
}