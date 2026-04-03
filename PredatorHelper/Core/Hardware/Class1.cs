using System.Management;

namespace PredatorHelper.Core.Hardware
{
    public class HardwareMonitor
    {
        private ManagementObject? _acerWmi;

        private ManagementObject GetAcerWmi()
        {
            if (_acerWmi != null) return _acerWmi;
            var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM AcerGamingFunction");
            _acerWmi = searcher.Get().Cast<ManagementObject>().FirstOrDefault()
                       ?? throw new InvalidOperationException("AcerGamingFunction WMI not found");
            return _acerWmi;
        }

        private int? GetSensorReading(ulong sensorId)
        {
            try
            {
                var obj = GetAcerWmi();
                var cmd = (ulong)(0x0001 | (sensorId << 8));
                var inParams = obj.GetMethodParameters("GetGamingSysInfo");
                inParams["gmInput"] = cmd;
                var outParams = obj.InvokeMethod("GetGamingSysInfo", inParams, null);
                var raw = (ulong)outParams["gmOutput"];
                if ((raw & 0xFF) == 0)
                    return (int)((raw >> 8) & 0xFFFF);
            }
            catch { _acerWmi = null; }
            return null;
        }

        public void Open() { }
        public void Close() { }

        public float? GetCpuTemperature() => (float?)GetSensorReading(0x01);
        public float? GetGpuTemperature() => (float?)GetSensorReading(0x0A);
        public int? GetCpuFanSpeed() => GetSensorReading(0x02);
        public int? GetGpuFanSpeed() => GetSensorReading(0x06);
    }
}
