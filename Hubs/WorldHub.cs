using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FFA.Hubs
{
    public class WorldHub : Hub
    {
        public async Task Subscribe() {
            // client can call to ensure connection
            await Task.CompletedTask;
        }
    }
}
