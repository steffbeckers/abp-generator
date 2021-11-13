using HandlebarsDotNet;
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
string templatesPath = Path.Combine(userBasedStoragePath, "Templates");
string webRootPath = Path.Combine("wwwroot", "public");

// Create user based directory if not exists and copy initial generator settings and templates
if (!Directory.Exists(userBasedStoragePath))
{
    Directory.CreateDirectory(userBasedStoragePath);
    File.Copy(
        Path.Combine(Directory.GetCurrentDirectory(), generatorSettingsFileName),
        Path.Combine(userBasedStoragePath, generatorSettingsFileName));

    Directory.CreateDirectory(templatesPath);
}

FileHelpers.CopyFilesRecursively("Templates", templatesPath);

// TODO: Extract to some template manager with a file watcher?
Dictionary<string, string> templates = new Dictionary<string, string>();
List<string> templateSettingFilePaths = Directory.GetFiles(templatesPath, "templatesettings.json", SearchOption.AllDirectories).ToList();
foreach (string? templateSettingFilePath in templateSettingFilePaths)
{
    if (templateSettingFilePath.Contains(Path.Combine(templatesPath, "Snippets")))
    {
        string templateFilePath = Path.Combine(Path.GetDirectoryName(templateSettingFilePath), "Template.hbs");
        if (File.Exists(templateFilePath))
        {
            string templateSource = File.ReadAllText(templateFilePath);
            HandlebarsTemplate<object, object>? template = Handlebars.Compile(templateSource);
            string? output = template(new
            {
                ProjectName = "MyCompany.MyProduct",
                AggregateRootName = "Test",
                AggregateRootNamePlural = "Tests"
            });

            templates.Add(Path.GetDirectoryName(templateSettingFilePath).Replace(templatesPath + Path.DirectorySeparatorChar, ""), output);
        }
    }
}

// TODO: Remove
foreach (KeyValuePair<string, string> template in templates)
{
    Console.WriteLine(template.Key);
    Console.WriteLine(template.Value);
    Console.WriteLine();
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
#if !DEBUG
    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
#endif
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
{
    BrowserHelper.OpenUrl(url);
}

await run;