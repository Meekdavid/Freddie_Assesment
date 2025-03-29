using Common.ConfigurationSettings.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ConfigurationSettings
{
    public class ConfigSettings
    {
        public static ConnectionStrings ConnectionString => ConfigurationSettingsHelper.GetConfigurationSectionObject<ConnectionStrings>("ConnectionStrings");
        public static ApplicationSettings ApplicationSetting => ConfigurationSettingsHelper.GetConfigurationSectionObject<ApplicationSettings>("ApplicationSettings");
    }
}
