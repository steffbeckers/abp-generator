using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using SteffBeckers.Abp.Generator.Templates;
using System.Diagnostics;
using System.Reflection;

Console.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = FileHelpers.ContentRootPath,
    WebRootPath = FileHelpers.WebRootPath
});

builder.Configuration.AddJsonFile(
    FileHelpers.UserBasedGeneratorSettingsFilePath,
    optional: true,
    reloadOnChange: true);

builder.Services
    .AddOptions<GeneratorSettings>()
    .Bind(builder.Configuration.GetSection("Generator"));
builder.Services.AddSingleton<SettingManager>();
builder.Services.AddSingleton<SnippetTemplateManager>();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

SettingManager? settingsManager = app.Services.GetRequiredService<SettingManager>();
await settingsManager.InitializeAsync();

SnippetTemplateManager? snippetTemplateManager = app.Services.GetRequiredService<SnippetTemplateManager>();
await snippetTemplateManager.InitializeAsync();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet(
    "/api/templates/open-folder",
    () =>
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = FileHelpers.UserBasedTemplatesPath,
            UseShellExecute = true,
            Verb = "open"
        });
    });

app.MapGet(
    "/api/templates/snippets",
    (SnippetTemplateManager templateManager) =>
    {
        return templateManager.Templates;
    });

app.MapGet(
    "/api/settings",
    (SettingManager settingsManager) =>
    {
        return settingsManager.Settings;
    });

app.MapPut(
    "/api/settings",
    async (GeneratorSettings input) =>
    {
        string json = JsonConvert.SerializeObject(
            new { Generator = input },
            Formatting.Indented);

        await File.WriteAllTextAsync(FileHelpers.UserBasedGeneratorSettingsFilePath, json);
    });

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<RealtimeHub>("/signalr-hubs/realtime");
});

Task run = app.RunAsync();

string? url = app.Urls.FirstOrDefault();
if (url != null)
{
    Console.WriteLine($"Navigating to: {url}");
    BrowserHelper.OpenUrl(url);
}

await run;