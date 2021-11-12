using Microsoft.AspNetCore.SignalR;

namespace SteffBeckers.Abp.Generator.Realtime
{
    public class RealtimeHub : Hub
    {
        private readonly ILogger<RealtimeHub> _logger;

        public RealtimeHub(ILogger<RealtimeHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("New realtime connection");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Realtime connection lost");

            return base.OnDisconnectedAsync(exception);
        }
    }
}
