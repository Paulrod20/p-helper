using LibreHardwareMonitor.Hardware;
using System.Management;

namespace PredatorHelper.Core.Hardware
{
    public class HardwareMonitor
    {
        private readonly Computer _computer;
        private ManagementObject? _acerWmi;

        public HardwareMonitor()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true
            };
        }

        public void Open() => _computer.Open();
        public void Close() => _computer.Close();

        private ManagementObject GetAcerWmi()
        {
            if (_acerWmi != null) return _acerWmi;
            var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM AcerGamingFunction");
            _acerWmi = searcher.Get().Cast<ManagementObject>().FirstOrDefault()
                       ?? throw new InvalidOperationException("AcerGamingFunction WMI not found");
            return _acerWmi;
        }

        public float? GetCpuTemperature()
        {
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                foreach (var subhardware in hardware.SubHardware)
                {
                    subhardware.Update();
                    foreach (var sensor in subhardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && sensor.Value > 0)
                            return sensor.Value;
                    }
                }
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && sensor.Value > 0 &&
                        (sensor.Name.Contains("CPU") || sensor.Name.Contains("Core") || sensor.Name.Contains("Package")))
                        return sensor.Value;
                }
            }
            return null;
        }

        public float? GetGpuTemperature()
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                            return sensor.Value;
                    }
                }
            }
            return null;
        }

        private int? GetFanSpeed(ulong sensorId)
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

        public int? GetCpuFanSpeed() => GetFanSpeed(0x02);
        public int? GetGpuFanSpeed() => GetFanSpeed(0x06);
    }
}