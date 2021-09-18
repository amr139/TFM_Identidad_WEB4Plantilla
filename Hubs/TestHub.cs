using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebAgent.Utils;

namespace WebAgent.Hubs
{
    public class TestHub : Hub
    {
        public Task JoinGroup(string group)
        {
            ShorterData t = MemoryDatabase.GetRegisterByWSID(group);
            if (t != null)
            {
                t.IncreaseClients();
                if (t.NumClients == 1)
                {
                    return Groups.AddToGroupAsync(Context.ConnectionId, group);
                }

            }
            return Task.CompletedTask;
            
        }

    }
}
