using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Settings;

string userBasedStoragePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".steffbeckers",
    "abp",
    "generator");
string generatorSettingsFileName = "generatorsettings.json";
string templatesPath = Path.Combine(userBasedStoragePath, "templates");
string webRootPath = Path.Combine("wwwroot", "public");

// Create user based directory if not exists and copy initial generator settings
if (!Directory.Exists(userBasedStoragePath))
{
    Directory.CreateDirectory(userBasedStoragePath);
    Directory.CreateDirectory(templatesPath);

    File.Copy(
        Path.Combine(Directory.GetCurrentDirectory(), generatorSettingsFileName),
        Path.Combine(userBasedStoragePath, generatorSettingsFileName));
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    WebRootPath = webRootPath
});

builder.Configuration.AddJsonFile(
    Path.Combine(userBasedStoragePath, generatorSettingsFileName),
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

        await File.WriteAllTextAsync(Path.Combine(userBasedStoragePath, generatorSettingsFileName), json);
    });

app.Run();
