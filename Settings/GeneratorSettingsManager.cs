using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SteffBeckers.Abp.Generator.Realtime;

namespace SteffBeckers.Abp.Generator.Settings;

public class GeneratorSettingsManager
{
    private readonly IOptionsMonitor<GeneratorSettings> _optionsMonitor;

    public GeneratorSettings CurrentValue { get { return _optionsMonitor.CurrentValue; } }

    public GeneratorSettingsManager(
        IHubContext<RealtimeHub> realtimeHub,
        IOptionsMonitor<GeneratorSettings> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        _optionsMonitor.OnChange(async settings =>
        {
            await realtimeHub.Clients.All.SendAsync("SettingsUpdated", settings);
        });
    }
}