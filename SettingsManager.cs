using System;
using System.IO;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    public static class SettingsManager
    {
        private static readonly string FilePath = "settings.json";

        public static void Save(Settings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                // In a real application, you might want to show this error to the user
                // or log it to a more persistent error log.
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonConvert.DeserializeObject<Settings>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings, creating new default settings. Error: {ex.Message}");
            }

            // If file doesn't exist or fails to load, create new default settings and save them.
            Settings defaultSettings = new Settings();
            Save(defaultSettings);
            return defaultSettings;
        }
    }
}
