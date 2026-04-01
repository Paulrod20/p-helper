using System.Text.Json;

namespace PredatorHelper.Core.Services
{
    public class AppSettings
    {
        public int BatteryLimit { get; set; } = 100;
        public string LastMode { get; set; } = string.Empty;
    }

    public class SettingsService
    {
        private static readonly string _path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PredatorHelper",
            "config.json"
        );

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_path)) return new AppSettings();
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch { return new AppSettings(); }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}