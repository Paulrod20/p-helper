using System.Management;

namespace PredatorHelper.Core.Services
{
    public class PerformanceModeService
    {
        private static readonly Dictionary<string, byte> _profileMap = new()
        {
            { "Silent",    0x00 },
            { "Balanced",  0x01 },
            { "Perform",   0x04 },
            { "Turbo",     0x05 },
            { "Eco",       0x06 },
        };

        private ManagementObject? _acerWmi;

        private ManagementObject GetAcerWmi()
        {
            if (_acerWmi != null) return _acerWmi;
            var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM AcerGamingFunction");
            _acerWmi = searcher.Get().Cast<ManagementObject>().FirstOrDefault()
                       ?? throw new InvalidOperationException("AcerGamingFunction WMI not found");
            return _acerWmi;
        }

        public bool SetMode(string modeName)
        {
            if (!_profileMap.TryGetValue(modeName, out byte profileValue))
                return false;

            try
            {
                var obj = GetAcerWmi();
                var inParams = obj.GetMethodParameters("SetGamingMiscSetting");
                ulong input = (ulong)0x0B | ((ulong)profileValue << 8);
                inParams["gmInput"] = input;
                var outParams = obj.InvokeMethod("SetGamingMiscSetting", inParams, null);
                var result = (ulong)outParams["gmOutput"];
                return (result & 0xFF) == 0;
            }
            catch { _acerWmi = null; }
            return false;
        }

        // Fan mode values from Linux kernel source (acer-wmi.c)
        // Auto = 0x01, Max = 0x02 (labeled "Turbo" internally), Custom = 0x03
        public bool SetFanMode(string fanMode)
        {
            byte mode = fanMode switch
            {
                "Auto" => 0x01,
                "Max" => 0x02, // WMI calls this Turbo internally
                "Custom" => 0x03,
                _ => 0x01
            };

            try
            {
                var obj = GetAcerWmi();
                var inParams = obj.GetMethodParameters("SetGamingFanBehavior");
                // 0x09 = CPU fan (bit 0) + GPU fan (bit 3)
                ulong input = (ulong)(0x09 | (mode << 16) | (mode << 22));
                inParams["gmInput"] = input;
                var outParams = obj.InvokeMethod("SetGamingFanBehavior", inParams, null);
                var result = (ulong)outParams["gmOutput"];
                return (result & 0xFF) == 0;
            }
            catch { _acerWmi = null; }
            return false;
        }

        public string? GetCurrentMode()
        {
            try
            {
                var obj = GetAcerWmi();
                var inParams = obj.GetMethodParameters("GetGamingMiscSetting");
                inParams["gmInput"] = (ulong)0x0B;
                var outParams = obj.InvokeMethod("GetGamingMiscSetting", inParams, null);
                var raw = (ulong)outParams["gmOutput"];

                if ((raw & 0xFF) != 0) return null;

                byte profileValue = (byte)((raw >> 8) & 0xFF);

                return profileValue switch
                {
                    0x00 => "Silent",
                    0x01 => "Balanced",
                    0x04 => "Perform",
                    0x05 => "Turbo",
                    0x06 => "Eco",
                    _ => null
                };
            }
            catch { _acerWmi = null; }
            return null;
        }
    }
}
