using PredatorHelper.Core.Hardware;

namespace PredatorHelper.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly HardwareMonitor _monitor;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly string[] _modes = { "Silent", "Balanced", "Perform", "Turbo", "Eco" };
        private NotifyIcon _trayIcon = null!;

        public MainForm()
        {
            InitializeComponent();
            ApplyTheme();
            BuildUI();
            UpdatePowerState();
            SetupTray();

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
            this.StartPosition = FormStartPosition.Manual;

            var screen = Screen.PrimaryScreen!.WorkingArea;
            this.Location = new Point(
                screen.Right - this.Width - 10,
                screen.Bottom - this.Height - 60
            );
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

            var fanTitle = new Label
            {
                Text = "🌀 Fan Speeds",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 235),
                AutoSize = true
            };

            var cpuFanLabel = new Label
            {
                Name = "cpuFanLabel",
                Text = "CPU Fan: -- RPM",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Silver,
                Location = new Point(20, 260),
                AutoSize = true
            };

            var gpuFanLabel = new Label
            {
                Name = "gpuFanLabel",
                Text = "GPU Fan: -- RPM",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Silver,
                Location = new Point(200, 260),
                AutoSize = true
            };

            this.Controls.AddRange(new Control[] { fanTitle, cpuFanLabel, gpuFanLabel });

            // --- Battery Charge Limit Section ---
            var batteryTitle = new Label {
                Text = "🔋 Battery Charge Limit",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 165),
                AutoSize = true
            };

            var batteryPercentLabel = new Label { 
                Name = "batteryPercentLabel",
                Text = $"Charge: {SystemInformation.PowerStatus.BatteryLifePercent * 100:F0}%",
                Font = new Font("Segoe UI", 9f),
                ForeColor= Color.Silver,
                Location = new Point(420, 228),
                AutoSize = true
            };

            var batteryValueLabel = new Label { 
                Name = "batteryValueLabel",
                Text = "100%",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Silver,
                Location = new Point(490, 165),
                AutoSize = true
            };

            var batterySlider = new TrackBar { 
                Name = "batterySlider",
                Minimum = 60,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10,
                LargeChange = 10,
                SmallChange = 5,
                Location = new Point(16, 188),
                Size = new Size(520, 40),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            batterySlider.ValueChanged += (s, e) =>
            {
                var label = this.Controls.Find("batteryValueLabel", false).FirstOrDefault();
                if (label != null)
                    label.Text = $"{batterySlider.Value}%";
            };

            this.Controls.AddRange(new Control[] { batteryTitle, batteryPercentLabel, batteryValueLabel, batterySlider });
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

            UpdatePowerState();

            var cpuFanLabel = this.Controls.Find("cpuFanLabel", false).FirstOrDefault();
            if (cpuFanLabel != null)
                cpuFanLabel.Text = $"CPU Fan: {_monitor.GetCpuFanSpeed() ?? 0} RPM";

            var gpuFanLabel = this.Controls.Find("gpuFanLabel", false).FirstOrDefault();
            if (gpuFanLabel != null)
                gpuFanLabel.Text = $"GPU Fan: {_monitor.GetGpuFanSpeed() ?? 0} RPM";

            var batteryPercenLabel = this.Controls.Find("batteryPercentLabel", false).FirstOrDefault();
            if (batteryPercenLabel != null)
                batteryPercenLabel.Text = $"Charge: {SystemInformation.PowerStatus.BatteryLifePercent * 100:F0}%";
        }

        private void UpdatePowerState()
        {
            bool isPluggedIn = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;

            var pluggedInOnly = new[] { "Silent", "Perform", "Turbo" };
            var batteryOnly = new[] { "Eco" };

            foreach (Control c in this.Controls)
            {
                if (c is not Button btn || !_modes.Contains(btn.Tag?.ToString())) continue;

                var mode = btn.Tag?.ToString();
                bool enabled = isPluggedIn
                    ? !batteryOnly.Contains(mode) 
                    : !pluggedInOnly.Contains(mode);

                btn.Enabled = enabled;
                btn.BackColor = enabled ? Color.FromArgb(50, 50, 50) : Color.FromArgb(35, 35, 35);
            }
        }

        private void SetupTray()
        {
            _trayIcon = new NotifyIcon
            {
                Text = "PredatorHelper",
                Icon = SystemIcons.Application,
                Visible = true
            };

            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, (s, e) => ShowWindow());
            trayMenu.Items.Add("Quit", null, (s, e) => Application.Exit());

            _trayIcon.ContextMenuStrip = trayMenu;
            _trayIcon.Click += (s, e) => ShowWindow();
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                var result = MessageBox.Show(
                    "Quit PredatorHelper?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes) { 
                    Application.Exit();
                }
            }
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
            base.OnResize(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _monitor.Close();
            _timer.Stop();
            _trayIcon.Dispose();
            base.OnFormClosed(e);
        }
    }
}