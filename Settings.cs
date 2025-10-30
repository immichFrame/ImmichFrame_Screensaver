using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImmichFrame_Screensaver
{
    public static class Settings
    {
        public static string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "ImmichFrame_Screensaver");
        private static readonly string SettingsFilePath = Path.Combine(appDataFolder, "Settings.json");

        private static AppSettings currentSettings;
        public static AppSettings CurrentSettings
        {
            get
            {
                if (currentSettings == null)
                {
                    currentSettings = LoadSettings();
                }
                return currentSettings;
            }
        }
        public static AppSettings LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            return new AppSettings(); // Return default settings if the file doesn't exist
        }

        public static void SaveSettings(AppSettings settings)
        {
            string directory = Path.GetDirectoryName(SettingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsFilePath, json);
            currentSettings = settings;
        }
    }
    public class AppSettings
    {
        private string _savedUrl = string.Empty;
        public string SavedUrl
        {
            get => _savedUrl;
            set
            {
                if (_savedUrl != value)
                {
                    _savedUrl = value;
                    Settings.SaveSettings(this);
                }
            }
        }
    }
}
