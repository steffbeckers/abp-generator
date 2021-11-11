using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Settings;

string contentRootPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.steffbeckers/abp/generator/";
string webRootPath = Path.Combine("wwwroot", "public");

// Create content root path if not exists and copy initial settings
if (!Directory.Exists(contentRootPath))
{
    Directory.CreateDirectory(contentRootPath);
    
    foreach (string jsonFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*settings.json"))
    {
        File.Copy(jsonFile, Path.Combine(contentRootPath, Path.GetFileName(jsonFile)));
    }
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    WebRootPath = webRootPath,
    ContentRootPath = contentRootPath
});

// Copy wwwroot/public/
FileHelpers.CopyFilesRecursively(
    Path.Combine(Directory.GetCurrentDirectory(), webRootPath),
    Path.Combine(contentRootPath, webRootPath));

builder.Configuration.AddJsonFile(
    Path.Combine(contentRootPath, "generatorsettings.json"),
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

        await File.WriteAllTextAsync(Path.Combine(contentRootPath, "generatorsettings.json"), json);
    });

app.Run();
