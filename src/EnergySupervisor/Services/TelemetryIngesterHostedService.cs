using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnergySupervisor.Cache;
using EnergySupervisor.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRChat.Hubs;
using Microsoft.Azure.EventHubs;

namespace EnergySupervisor.Services
{
    public class TelemetryIngesterHostedService : IHostedService
    {
        private readonly IServiceProvider services;
//
//        private static readonly string s_eventHubsCompatibleEndpoint = "";
//
//        // Event Hub-compatible name
//        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
//        private static readonly string s_eventHubsCompatiblePath = "";
//        
//        // az iot hub policy show --name iothubowner --query primaryKey --hub-name {your IoT Hub name}
//        private static readonly string s_iotHubSasKey = "";
//        private static readonly string s_iotHubSasKeyName = "service";
        private static EventHubClient s_eventHubClient;
        //private readonly IoTHubConfiguration iotHubConfiguration;
        
        public TelemetryIngesterHostedService(IServiceProvider services)
            => this.services = services;
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await DoWork();

            await Task.CompletedTask;
        }

        private async Task DoWork()
        {
            Console.WriteLine("IoT Hub Quickstarts - Read device to cloud messages. Ctrl-C to exit.\n");
            var iotHubConfiguration = services.GetRequiredService<IoTHubConfiguration>();
            // Create an EventHubClient instance to connect to the
            // IoT Hub Event Hubs-compatible endpoint.
            var connectionString = new EventHubsConnectionStringBuilder(new Uri(iotHubConfiguration.EventHubsCompatibleEndpoint), iotHubConfiguration.EventHubsCompatiblePath, iotHubConfiguration.IotHubSasKeyName, iotHubConfiguration.IotHubSasKey);
            s_eventHubClient = EventHubClient.CreateFromConnectionString(connectionString.ToString());

            // Create a PartitionReciever for each partition on the hub.
            var runtimeInfo = await s_eventHubClient.GetRuntimeInformationAsync();
            var d2cPartitions = runtimeInfo.PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }

            // Wait for all the PartitionReceivers to finsih.
            Task.WaitAll(tasks.ToArray());
        }
        
        
        private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {

            using (var scope = services.CreateScope())
            {
                var hubContext =
                    scope.ServiceProvider
                        .GetRequiredService<IHubContext<PageUpdateHub>>();
                var deviceController = scope.ServiceProvider.GetRequiredService<DeviceController>();
                // Create the receiver using the default consumer group.
                // For the purposes of this sample, read only messages sent since 
                // the time the receiver is created. Typically, you don't want to skip any messages.
                var eventHubReceiver = s_eventHubClient.CreateReceiver("$Default", partition,
                    EventPosition.FromEnqueuedTime(DateTime.Now));
                Console.WriteLine("Create receiver on partition: " + partition);
                while (true)
                {
                    if (ct.IsCancellationRequested) break;
                    // Console.WriteLine("Listening for messages on: " + partition);
                    // Check for EventData - this methods times out if there is nothing to retrieve.
                    var events = await eventHubReceiver.ReceiveAsync(100);

                    // If there is data in the batch, process it.
                    if (events == null) continue;

                    foreach (EventData eventData in events)
                    {
                        string data = Encoding.UTF8.GetString(eventData.Body.Array);
                        Console.WriteLine($"{DateTime.Now} New message!!! {data}");
//                        Console.WriteLine("Message received on partition {0}:", partition);
//                        Console.WriteLine("  {0}:", data);
//                        Console.WriteLine("Application properties (set by device):");
//
//                        foreach (var prop in eventData.Properties)
//                        {
//                            Console.WriteLine("  {0}: {1}", prop.Key, prop.Value);
//                        }
//                        Console.WriteLine("System properties (set by IoT Hub):");
//                        foreach (var prop in eventData.SystemProperties)
//                        {
//                            Console.WriteLine("  {0}: {1}", prop.Key, prop.Value);
//                        }
                        
                        if (eventData.Properties.Any(p =>
                            string.Equals(p.Key, "telemetryType", StringComparison.InvariantCultureIgnoreCase)
                            && string.Equals(p.Value.ToString(), "powerGeneration",
                                StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (double.TryParse(data, out double powerGenerated))
                            {
                                await hubContext.Clients.All.SendAsync("PowerGeneratorUpdate", powerGenerated, ct);
                                await deviceController.ConsumePower(powerGenerated);
                            }
                        }
                        else if (eventData.Properties.Any(p =>
                            string.Equals(p.Key, "telemetryType", StringComparison.InvariantCultureIgnoreCase)
                            && string.Equals(p.Value.ToString(), "powerConsumer",
                                StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var isLedOn = string.Equals(data, "on", StringComparison.InvariantCultureIgnoreCase);
                            var deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
                            //Console.WriteLine($"Device: {deviceId} Led status {isLedOn}");
                            DevicesLastState.UpdateDeviceState(deviceId, isLedOn);
                            await hubContext.Clients.All.SendAsync("LedStatusUpdate", deviceId, isLedOn, ct);
                        }
                    }
                }
            }
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}