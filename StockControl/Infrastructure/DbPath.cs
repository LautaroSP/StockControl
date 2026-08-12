using System.Configuration;

namespace StockControl.Infrastructure
{
    public static class DbPath
    {
        public const string SettingsKey = "DatabasePath";

        public static string DefaultPath =>
            Path.Combine(AppContext.BaseDirectory, "stock.db");

        public static string CurrentPath { get; private set; } = DefaultPath;

        public static bool IsUsingDefault =>
            string.Equals(
                Path.GetFullPath(CurrentPath),
                Path.GetFullPath(DefaultPath),
                StringComparison.OrdinalIgnoreCase);

        public static string ConnectionString =>
            $"Data Source={CurrentPath}";

        public static void Load()
        {
            string? configured = ConfigurationManager.AppSettings[SettingsKey];
            if (string.IsNullOrWhiteSpace(configured))
            {
                CurrentPath = DefaultPath;
                return;
            }

            CurrentPath = Path.GetFullPath(configured.Trim());
        }

        /// <summary>
        /// Guarda la ruta en App.config. Pasar null o vacío para volver a la raíz.
        /// </summary>
        public static void Save(string? path)
        {
            string valueToStore = string.IsNullOrWhiteSpace(path) || IsDefaultPath(path)
                ? string.Empty
                : Path.GetFullPath(path.Trim());

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;

            if (settings[SettingsKey] == null)
                settings.Add(SettingsKey, valueToStore);
            else
                settings[SettingsKey].Value = valueToStore;

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            CurrentPath = string.IsNullOrEmpty(valueToStore) ? DefaultPath : valueToStore;
        }

        public static bool IsDefaultPath(string path) =>
            string.Equals(
                Path.GetFullPath(path),
                Path.GetFullPath(DefaultPath),
                StringComparison.OrdinalIgnoreCase);

        public static string DisplayPath =>
            IsUsingDefault ? $"Raíz ({DefaultPath})" : CurrentPath;
    }
}
