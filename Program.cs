using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using SteffBeckers.Abp.Generator.Templates;
using SteffBeckers.Abp.Generator.Updates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(
    new WebApplicationOptions()
    {
        Args = args,
        ContentRootPath = FileHelpers.ContentRootPath,
        WebRootPath = FileHelpers.WebRootPath
    });

builder.Services.AddSingleton<UpdateService>();

builder.Configuration.AddJsonFile(FileHelpers.UserBasedGeneratorSettingsFilePath, optional: true, reloadOnChange: true);
builder.Services.AddOptions<GeneratorSettings>().Bind(builder.Configuration.GetSection("Generator"));
builder.Services.AddSingleton<SettingsService>();

builder.Services.AddSingleton<ProjectTemplatesService>();
builder.Services.AddSingleton<SnippetTemplatesService>();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

Console.WriteLine("ABP.io Generator");

UpdateService? updateService = app.Services.GetRequiredService<UpdateService>();
await updateService.CheckForUpdateAsync(app.Lifetime.ApplicationStopping);

SettingsService? settingsService = app.Services.GetRequiredService<SettingsService>();
await settingsService.InitializeAsync();

ProjectTemplatesService? projectTemplatesService = app.Services.GetRequiredService<ProjectTemplatesService>();
await projectTemplatesService.InitializeAsync();

SnippetTemplatesService? snippetTemplatesService = app.Services.GetRequiredService<SnippetTemplatesService>();
await snippetTemplatesService.InitializeAsync();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/version", () => updateService.Version);

app.MapGet("/api/settings", () => settingsService.GetAsync());
app.MapPut("/api/settings", (GeneratorSettings input) => settingsService.UpdateAsync(input));
app.MapGet("/api/settings/open-json", () => settingsService.OpenJsonAsync());
app.MapGet("/api/settings/open-project-folder", () => settingsService.OpenProjectFolderAsync());

app.MapGet("/api/templates/projects", () => projectTemplatesService.GetListAsync());
app.MapGet("/api/templates/projects/open-folder", () => projectTemplatesService.OpenFolderAsync());
app.MapPost(
    "/api/templates/projects/generate",
    (ProjectTemplateGenerateInputDto input) => projectTemplatesService.GenerateAsync(input));

app.MapGet("/api/templates/snippets", () => snippetTemplatesService.GetListAsync());
app.MapGet("/api/templates/snippets/open-folder", () => snippetTemplatesService.OpenFolderAsync());
app.MapPost(
    "/api/templates/snippets/edit",
    (SnippetTemplateEditInputDto input) => snippetTemplatesService.EditAsync(input));
app.MapPost(
    "/api/templates/snippets/generate",
    (SnippetTemplateGenerateInputDto input) => snippetTemplatesService.GenerateAsync(input));

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapHub<RealtimeHub>("/signalr-hubs/realtime"));

Task run = app.RunAsync();

string? url = app.Urls.FirstOrDefault();
if (url != null)
{
    Console.WriteLine($"Navigating to: {url}");
    BrowserHelpers.OpenUrl(url);
}

await run;