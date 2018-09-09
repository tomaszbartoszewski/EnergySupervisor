using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace EnergySupervisor.Services
{
    public class DeviceController
    {
        private ServiceClient serviceClient;
        private string connectionString = "";

        public DeviceController()
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        }

        public async Task ConsumePower(double availablePower)
        {
            if (availablePower > 0)
            {
                await SendCloudToDeviceMessageAsync("turn on");
            }
            else
            {
                await SendCloudToDeviceMessageAsync("turn off");
            }
        }
        
        private async Task SendCloudToDeviceMessageAsync(string commandText)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(commandText));
            await serviceClient.SendAsync("317ce704-d7a1-4f17-bda6-a9dbf48403e8", commandMessage);
        }
    }
}