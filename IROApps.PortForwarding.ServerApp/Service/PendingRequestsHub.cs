﻿using System.Threading.Tasks;
using IRO.Mvc.Core.Dto;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace IROApps.PortForwarding.ServerApp.Service
{
    public class PendingRequestsHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}