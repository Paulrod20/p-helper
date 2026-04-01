using System.Management;

namespace PredatorHelper.Core.Services
{
    public class PerformanceModeService
    {
        // Values from Linux kernel source (acer-wmi.c)
        private static readonly Dictionary<string, byte> _profileMap = new()
        {
            { "Silent",    0x00 },
            { "Balanced",  0x01 },
            { "Perform",   0x04 },
            { "Turbo",     0x05 },
            { "Eco",       0x06 },
        };

        public bool SetMode(string modeName)
        {
            if (!_profileMap.TryGetValue(modeName, out byte profileValue))
                return false;

            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM AcerGamingFunction");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var inParams = obj.GetMethodParameters("SetGamingMiscSetting");
                    // Index 0x0B = platform profile, value = profile byte
                    ulong input = (ulong)0x0B | ((ulong)profileValue << 8);
                    inParams["gmInput"] = input;
                    var outParams = obj.InvokeMethod("SetGamingMiscSetting", inParams, null);
                    var result = (ulong)outParams["gmOutput"];
                    return (result & 0xFF) == 0;
                }
            }
            catch { }
            return false;
        }
    }
}