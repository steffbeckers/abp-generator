using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Settings;

string generatorRootPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.steffbeckers/abp/generator/";

if (!Directory.Exists(generatorRootPath))
{
    Directory.CreateDirectory(generatorRootPath);
    
    foreach (string jsonFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.json"))
    {
        File.Copy(jsonFile, Path.Combine(generatorRootPath, Path.GetFileName(jsonFile)));
    }
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    WebRootPath = "wwwroot/public/"
});

builder.Configuration.AddJsonFile(
    Path.Combine(generatorRootPath, "generatorsettings.json"),
    optional: true,
    reloadOnChange: true);

builder.Services
    .AddOptions<GeneratorSettings>()
    .Bind(builder.Configuration.GetRequiredSection("Generator"));

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet(
    "/api/settings",
    (IOptionsMonitor<GeneratorSettings> settings) => {
        return settings.CurrentValue;
    });

app.MapPut(
    "/api/settings",
    async (GeneratorSettings input) =>
    {
        string json = JsonConvert.SerializeObject(
            new { Generator = input },
            Formatting.Indented);

        await File.WriteAllTextAsync(Path.Combine(generatorRootPath, "generatorsettings.json"), json);
    });

app.Run();
