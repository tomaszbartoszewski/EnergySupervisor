using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRChat.Hubs;

namespace EnergySupervisor.Services
{
    public class FakeTelemetryIngesterHostedService : IHostedService
    {
        private int sleepTime = 1000;
        
        private readonly IServiceProvider services;
        
        public FakeTelemetryIngesterHostedService(IServiceProvider services)
            => this.services = services;
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DoWork();

            await Task.CompletedTask;
        }

        private void DoWork()
        {   
            using (var scope = services.CreateScope())
            {
                var hubContext = 
                    scope.ServiceProvider
                        .GetRequiredService<IHubContext<PageUpdateHub>>();

                while (true)
                {
                    hubContext.Clients.All.SendAsync("PowerGeneratorUpdate", 2.3);
                    Thread.Sleep(sleepTime);
                    hubContext.Clients.All.SendAsync("LedStatusUpdate", "317ce704-d7a1-4f17-bda6-a9dbf48403e8", true);
                    Thread.Sleep(sleepTime);
                    hubContext.Clients.All.SendAsync("PowerGeneratorUpdate", 3.3);
                    Thread.Sleep(sleepTime);
                    hubContext.Clients.All.SendAsync("LedStatusUpdate", "10b65141-ded1-4109-86ea-c49938fd28e4", true);
                    Thread.Sleep(sleepTime);
                    hubContext.Clients.All.SendAsync("PowerGeneratorUpdate", 1.3);
                    Thread.Sleep(sleepTime);
                    hubContext.Clients.All.SendAsync("LedStatusUpdate", "10b65141-ded1-4109-86ea-c49938fd28e4", false);
                    Thread.Sleep(sleepTime);
                    hubContext.Clients.All.SendAsync("PowerGeneratorUpdate", 0);
                    Thread.Sleep(sleepTime);
                    hubContext.Clients.All.SendAsync("LedStatusUpdate", "317ce704-d7a1-4f17-bda6-a9dbf48403e8", false);
                    Thread.Sleep(sleepTime);
                }
            }
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}