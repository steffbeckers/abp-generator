using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using SteffBeckers.Abp.Generator.Templates;
using System.Reflection;

string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

Console.WriteLine($"Version: {version}");

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
builder.Services.AddSingleton<SettingsAppService>();
builder.Services.AddSingleton<SnippetTemplatesAppService>();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

SettingsAppService? settingsAppService = app.Services.GetRequiredService<SettingsAppService>();
await settingsAppService.InitializeAsync();

SnippetTemplatesAppService? snippetTemplatesAppService = app.Services.GetRequiredService<SnippetTemplatesAppService>();
await snippetTemplatesAppService.InitializeAsync();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/version", () => version);

app.MapGet("/api/settings", () => settingsAppService.GetAsync());
app.MapPut("/api/settings", (GeneratorSettings input) => settingsAppService.UpdateAsync(input));

app.MapGet("/api/templates/snippets", () => snippetTemplatesAppService.GetListAsync());
app.MapGet("/api/templates/snippets/open-folder", () => snippetTemplatesAppService.OpenFolderAsync());

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