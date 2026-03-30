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
            };
        }

        public void Open() => _computer.Open();
        public void Close() => _computer.Close();

        public float? GetCpuTemperature()
        {
            foreach (var hardware in _computer.Hardware) 
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors) 
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name == "CPU Package")
                            return sensor.Value;
                    }
                }
            }
            return null;
        }

        public float? GetGpuTemperature()
        {
            foreach (var hardware in _computer.Hardware) 
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if(sensor.SensorType == SensorType.Temperature)
                            return sensor.Value;
                    }
                }
            }
            return null;
        }
    }
}
