using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergySupervisor.Cache;
using EnergySupervisor.Domain;
using Microsoft.Azure.Devices;

namespace EnergySupervisor.Services
{
    public class DeviceController
    {
        private readonly ServiceClient serviceClient;

        private const double PowerPerDevice = 1.5d;

        //private readonly IoTHubConfiguration iotHubConfiguration;
        
        public DeviceController(IoTHubConfiguration iotHubConfiguration)
        {
            //this.iotHubConfiguration = iotHubConfiguration;
            serviceClient = ServiceClient.CreateFromConnectionString(iotHubConfiguration.IotHubConnectionString);
        }
        
        public async Task ConsumePower(double availablePower)
        {
            Console.WriteLine($"New power {availablePower}");
            if (availablePower > 0)
            {
                var numberOfDevicesToBeOn = (int)Math.Ceiling(availablePower / PowerPerDevice);
                var numberOfDevicesOn = DevicesLastState.GetAllTurnedOnDevices().Length;
                var numberOfDevicesToTurnOn = numberOfDevicesToBeOn - numberOfDevicesOn;
                Console.WriteLine($"Devices to change {numberOfDevicesToTurnOn}");
                if (numberOfDevicesToTurnOn > 0)
                    await SendCloudToDeviceMessageAsync("turn on", DevicesLastState.GetAllTurnedOffDevices().Take(numberOfDevicesToTurnOn).ToArray());
                else
                    await SendCloudToDeviceMessageAsync("turn off", DevicesLastState.GetAllTurnedOnDevices().Take(-numberOfDevicesToTurnOn).ToArray());
            }
            else
            {
                await SendCloudToDeviceMessageAsync("turn off", DevicesLastState.GetAllTurnedOnDevices());
            }
        }
        
        private async Task SendCloudToDeviceMessageAsync(string commandText, string[] devices)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(commandText));
            foreach (var device in devices)
            {
                Console.WriteLine($"{commandText} {device}");
//                var serviceClient = ServiceClient.CreateFromConnectionString(iotHubConfiguration.IotHubConnectionString);
                await serviceClient.SendAsync(device, commandMessage);
                //await Task.Delay(2000);
            }
        }
    }
}