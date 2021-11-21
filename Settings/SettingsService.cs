using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;

namespace SteffBeckers.Abp.Generator.Settings;

public class SettingsService
{
    public readonly IOptionsMonitor<GeneratorSettings> Monitor;

    public GeneratorSettings Settings { get => Monitor.CurrentValue; }

    public SettingsService(
        IHubContext<RealtimeHub> realtimeHub,
        IOptionsMonitor<GeneratorSettings> optionsMonitor)
    {
        Monitor = optionsMonitor;

        Monitor.OnChange(async settings =>
        {
            await realtimeHub.Clients.All.SendAsync("SettingsUpdated", settings);
        });
    }

    public Task<GeneratorSettings> GetAsync()
    {
        return Task.FromResult(Settings);
    }

    public Task InitializeAsync()
    {
        if (!Directory.Exists(FileHelpers.UserBasedPath))
        {
            Directory.CreateDirectory(FileHelpers.UserBasedPath);

            File.Copy(
                Path.Combine(FileHelpers.ContentRootPath, FileHelpers.GeneratorSettingsFileName),
                Path.Combine(FileHelpers.UserBasedPath, FileHelpers.GeneratorSettingsFileName));
        }

        return Task.CompletedTask;
    }

    public async Task UpdateAsync(GeneratorSettings input)
    {
        string json = JsonConvert.SerializeObject(new { Generator = input }, Formatting.Indented);

        await File.WriteAllTextAsync(FileHelpers.UserBasedGeneratorSettingsFilePath, json);
    }
}