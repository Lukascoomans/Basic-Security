using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Encryption.Server
{
    [HubName("chat")]
    public class ChatHub : Hub
    {
        public void SendMessage(string message)
        {
            Clients.Others.newMessage(message);
        }

        public void SetUserName(string username)
        {
            Clients.Others.SetUsername(username);
        }
        public void namesConfig(string username)
        {
            Clients.Others.namesConfig(username);
        }
        public void callNames(string username)
        {
            Clients.Others.callNames(username);
        }
        
    }
}