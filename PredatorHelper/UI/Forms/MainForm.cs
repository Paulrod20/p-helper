using PredatorHelper.Core.Hardware;

namespace PredatorHelper.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly HardwareMonitor _monitor;
        private readonly System.Windows.Forms.Timer _timer;

        public MainForm()
        {
            InitializeComponent();
            _monitor = new HardwareMonitor();
            _monitor.Open();

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;
            _timer.Tick += UpdateReadings;
            _timer.Start();
        }

        private void UpdateReadings(object? sender, EventArgs e)
        {
            var cpuTemp = _monitor.GetCpuTemperature();
            var gpuTemp = _monitor.GetGpuTemperature();

            this.Text = $"PredatorHelper | CPU: {cpuTemp:F1}°C | GPU: {gpuTemp:F1}°C";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _monitor.Close();
            _timer.Stop();
            base.OnFormClosed(e);
        }
    }
}