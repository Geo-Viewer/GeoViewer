using System;
using System.IO;
using GeoViewer.Model.State;
using Newtonsoft.Json;
using UnityEngine;

namespace GeoViewer.Controller.Files
{
    /// <summary>
    /// a class to get application settings from configuration file.
    /// </summary>
    public static class ConfigLoader
    {
        private const string RelativeConfigPath = "GeoViewer";
        private const string ConfigName = "config.json";
        private const string BackupName = "config-old.json";

        private static readonly string ConfigPath;
        private static readonly string OldConfigPath;

        static ConfigLoader()
        {
            ConfigPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                RelativeConfigPath,
                ConfigName
            );
            OldConfigPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                RelativeConfigPath,
                BackupName
            );
        }

        /// <summary>
        /// Returns an instance of ApplicationSettings based on ConfigPath.
        /// If there were problems while creating or loading the config, the error is logged and the default
        /// configuration returned.
        /// </summary>
        public static ApplicationSettings GetSettingsFromConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    Debug.LogWarning($"There is no file at {ConfigPath}. Creating default config.");
                    // create new default ConfigFile in Json format at ConfigPath
                    SaveConfig(new ApplicationSettings());
                }

                var str = File.ReadAllText(ConfigPath);
                var settings = JsonConvert.DeserializeObject<ApplicationSettings>(str, new JsonSerializerSettings()
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                });

                switch (settings!.ConfigVersion)
                {
                    // check config version
                    case < ApplicationSettings.SettingsVersion:
                        Debug.Log(
                            $"Switching from config version {settings.ConfigVersion} to config version {ApplicationSettings.SettingsVersion}");

                        settings!.ConfigVersion = ApplicationSettings.SettingsVersion;

                        BackupConfig();
                        SaveConfig(settings);
                        break;
                    case > ApplicationSettings.SettingsVersion:
                        Debug.LogWarning(
                            $"Saved Config Version ({settings!.ConfigVersion}) is higher than current version ({ApplicationSettings.SettingsVersion})");
                        break;
                }

                return settings;
            }
            catch (IOException e)
            {
                // If there were any problems, log them and return the default config.
                Debug.LogError(e);
            }

            return new ApplicationSettings();
        }

        private static void BackupConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                return;
            }

            if (File.Exists(OldConfigPath))
            {
                File.Delete(OldConfigPath);
            }

            File.Move(ConfigPath, OldConfigPath);
        }

        private static void SaveConfig(ApplicationSettings settings)
        {
            if (settings.ConfigVersion > ApplicationSettings.SettingsVersion)
            {
                Debug.LogWarning(
                    $"Prevented Config with version {settings.ConfigVersion} from being saved, due to config version being higher than current settings version");
                return;
            }

            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir) && dir != null)
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}