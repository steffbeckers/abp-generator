using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;

namespace SteffBeckers.Abp.Generator.Settings;

public class SettingManager
{
    public readonly IOptionsMonitor<GeneratorSettings> Monitor;

    public GeneratorSettings Settings { get => Monitor.CurrentValue; }

    public SettingManager(
        IHubContext<RealtimeHub> realtimeHub,
        IOptionsMonitor<GeneratorSettings> optionsMonitor)
    {
        Monitor = optionsMonitor;

        Monitor.OnChange(async settings =>
        {
            await realtimeHub.Clients.All.SendAsync("SettingsUpdated", settings);
        });
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
}