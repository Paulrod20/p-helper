using PredatorHelper.Core.Hardware;

namespace PredatorHelper.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly HardwareMonitor _monitor;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly string[] _modes = { "Silent", "Balanced", "Perform", "Turbo", "Eco" };

        public MainForm()
        {
            InitializeComponent();
            ApplyTheme();
            BuildUI();

            _monitor = new HardwareMonitor();
            _monitor.Open();

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;
            _timer.Tick += UpdateReadings;
            _timer.Start();
        }

        private void ApplyTheme()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(560, 600);
            this.Text = "PredatorHelper";
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void BuildUI()
        {
            var headerLabel = new Label
            {
                Text = "PredatorHelper",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var tempLabel = new Label
            {
                Name = "tempLabel",
                Text = "CPU: --°C   GPU: --°C",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Silver,
                Location = new Point(300, 15),
                AutoSize = true
            };

            var modeTitle = new Label
            {
                Text = "⚡ Performance Mode",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 58),
                AutoSize = true
            };

            int btnX = 16;
            foreach (var mode in _modes)
            {
                var btn = new Button
                {
                    Text = mode,
                    Size = new Size(96, 55),
                    Location = new Point(btnX, 90),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(50, 50, 50),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f),
                    Cursor = Cursors.Hand,
                    Tag = mode
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += ModeButton_Click;
                this.Controls.Add(btn);
                btnX += 100;
            }

            this.Controls.AddRange(new Control[] { headerLabel, tempLabel, modeTitle });
        }

        private void ModeButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button clicked) return;

            foreach (Control c in this.Controls)
            {
                if (c is Button btn && _modes.Contains(btn.Tag?.ToString()))
                {
                    btn.BackColor = Color.FromArgb(50, 50, 50);
                    btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                }
            }

            clicked.BackColor = Color.FromArgb(0, 120, 80);
            clicked.FlatAppearance.BorderColor = Color.FromArgb(0, 180, 120);
        }

        private void UpdateReadings(object? sender, EventArgs e)
        {
            var cpuTemp = _monitor.GetCpuTemperature();
            var gpuTemp = _monitor.GetGpuTemperature();

            var tempLabel = this.Controls.Find("tempLabel", false).FirstOrDefault();
            if (tempLabel != null)
                tempLabel.Text = $"CPU: {cpuTemp:F1}°C   GPU: {gpuTemp:F1}°C";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _monitor.Close();
            _timer.Stop();
            base.OnFormClosed(e);
        }
    }
}