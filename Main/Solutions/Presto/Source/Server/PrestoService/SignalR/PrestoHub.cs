using Microsoft.AspNet.SignalR;

namespace PrestoWcfService.SignalR
{
    public class PrestoHub : Hub {}

    // Note: Our hub doesn't need any methods. When we want to send a message, we do so
    //       using the hub context. Something like this:
    //
    // var hubContext = GlobalHost.ConnectionManager.GetHubContext<PrestoHub>();
    // hubContext.Clients.All.OnSignalRMessage("snuh");
}
