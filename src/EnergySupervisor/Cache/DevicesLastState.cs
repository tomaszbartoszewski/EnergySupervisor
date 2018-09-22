using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EnergySupervisor.Cache
{
    public static class DevicesLastState
    {
        // values are not important for those collections, I just wanted something thread safe which will keep a list of devices
        private static readonly ConcurrentDictionary<string, bool> devicesOn = new ConcurrentDictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly ConcurrentDictionary<string, bool> devicesOff = new ConcurrentDictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

        public static void UpdateDeviceState(string deviceId, bool state)
        {
            Console.WriteLine($"Update {deviceId} state {state}");
            
            if (state)
            {
                devicesOff.TryRemove(deviceId, out _);
                devicesOn.AddOrUpdate(deviceId, true, (key, oldValue) => true);
            }
            else
            {
                devicesOn.TryRemove(deviceId, out _);
                devicesOff.AddOrUpdate(deviceId, false, (key, oldValue) => false);
            }
        }

        public static string[] GetAllTurnedOffDevices()
        {
            return devicesOff.Keys.ToArray();
        }
        
        public static string[] GetAllTurnedOnDevices()
        {
            return devicesOn.Keys.ToArray();
        }
        
    }
}