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
