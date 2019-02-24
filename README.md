# EnergySupervisor

Project for controlling devices, it controls devices which run code from here https://github.com/tomaszbartoszewski/LED-RaspberryPi-Python

Currently it will read telemetry from devices, display power generated and if there is some power generated, it will send command to one device to turn LED on.

To run it, you will have to add IoT Hub details to appsettings.json IotHubConnectionString from Shared access policies, all other values come from page Build-in endpoints.
EventHubsCompatibleEndpoint, EventHubsCompatiblePath, IotHubSasKey, IotHubSasKeyName come from Event hub compatible endpoint.

To run it you should be able to just run

```
dotnet restore; dotnet build; dotnet run
```
