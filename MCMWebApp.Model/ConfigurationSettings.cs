using System.IO;
using Microsoft.Extensions.Configuration;

namespace MCMWebApp.Model
{
    public class ConfigurationSettings
    {
        private static ConfigurationSettings _instance = null;
        private static readonly object padlock = new object();
        private static IConfiguration configuration;

        #region Object Creation
        private ConfigurationSettings()
        {
            configuration = new ConfigurationBuilder()
                    //.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
        }

        public static ConfigurationSettings Instance
        {
            get
            {
                lock (padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new ConfigurationSettings();
                    }
                    return _instance;
                }
            }
        }

        #endregion
         

        public string AzureStorageSettings_StorageConnectionString
        {
            get => configuration.GetValue<string>("AzureDBStorageSettings:StorageConnectionString");
        }

        public string AzureStorageSettings_EventImageContainerName
        {
            get => configuration.GetValue<string>("AzureDBStorageSettings:EventImageContainerName");
        }

        public string AzureStorageSettings_VenueImageContainerName
        {
            get => configuration.GetValue<string>("AzureDBStorageSettings:VenueImageContainerName");
        }
    }
}
