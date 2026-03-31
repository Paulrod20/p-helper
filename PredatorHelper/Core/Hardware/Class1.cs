using LibreHardwareMonitor.Hardware;

namespace PredatorHelper.Core.Hardware
{
    public class HardwareMonitor
    {
        private readonly Computer _computer;

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
    }
}