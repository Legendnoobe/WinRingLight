using System;
using System.Management;

namespace WinRingLight;

public static class BrightnessHelper
{
    public static void SetBrightness(int brightness)
    {
        try
        {
            // Clamp brightness to 0-100
            byte target = (byte)Math.Clamp(brightness, 0, 100);
            
            var mclass = new ManagementClass("root\\wmi", "WmiMonitorBrightnessMethods", null);
            var instances = mclass.GetInstances();

            foreach (ManagementObject instance in instances)
            {
                instance.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, target });
                break; // Apply to first monitor found
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting brightness: {ex.Message}");
        }
    }
}
