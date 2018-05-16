using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Encryption.web
{
    [HubName("chat")]
    public class ChatHub : Hub
    {
        public void SendMessage(String message)
        {
            Clients.All.newMessage(message);
        }
    }
}