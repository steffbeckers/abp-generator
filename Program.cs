using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using System.Diagnostics;

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

// TODO: Templates folder file watcher?

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
builder.Services.AddSingleton<GeneratorSettingsManager>();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet(
    "/api/actions/open-templates-folder",
    () =>
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = templatesPath,
            UseShellExecute = true,
            Verb = "open"
        });
    });

app.MapGet(
    "/api/settings",
    (GeneratorSettingsManager settingsManager) =>
    {
        return settingsManager.CurrentValue;
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

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<RealtimeHub>("/signalr-hubs/realtime");
});

Task run = app.RunAsync();

// Open browser on startup
string? url = app.Urls.FirstOrDefault();
if (url != null)
    BrowserHelper.OpenUrl(url);

await run;
