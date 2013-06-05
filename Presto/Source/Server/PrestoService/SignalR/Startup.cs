using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.SignalR;
using Owin;

namespace PrestoWcfService.SignalR
{
    public class Startup
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Configuration(IAppBuilder app)
        {
            var config = new HubConfiguration { EnableCrossDomain = true };
            app.MapHubs(config);

            // I'm not sure if EnableCrossDomain (and therefore config) is necessary.
            // Once this is deployed in a distributed environment, it would be good to
            // replace the above code with this call to see if it works:
            // app.MapHubs();
        }
    }
}
