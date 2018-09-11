namespace EnergySupervisor.Domain
{
    public class IoTHubConfiguration
    {
        public string IotHubConnectionString { get; set; }
        public string EventHubsCompatibleEndpoint { get; set; }
        public string EventHubsCompatiblePath { get; set; }
        public string IotHubSasKey { get; set; }
        public string IotHubSasKeyName { get; set; }
    }
}