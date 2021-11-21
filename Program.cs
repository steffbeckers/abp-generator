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
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<SnippetTemplatesService>();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

SettingsService? settingsService = app.Services.GetRequiredService<SettingsService>();
await settingsService.InitializeAsync();

SnippetTemplatesService? snippetTemplatesService = app.Services.GetRequiredService<SnippetTemplatesService>();
await snippetTemplatesService.InitializeAsync();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/version", () => version);

app.MapGet("/api/settings", () => settingsService.GetAsync());
app.MapPut("/api/settings", (GeneratorSettings input) => settingsService.UpdateAsync(input));
app.MapGet("/api/settings/open-json", () => settingsService.OpenJsonAsync());

app.MapGet("/api/templates/snippets", () => snippetTemplatesService.GetListAsync());
app.MapGet("/api/templates/snippets/open-folder", () => snippetTemplatesService.OpenFolderAsync());
app.MapPost("/api/templates/snippets/generate", (GenerateSnippetTemplates input) => snippetTemplatesService.GenerateAsync(input));

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
    BrowserHelpers.OpenUrl(url);
}

await run;