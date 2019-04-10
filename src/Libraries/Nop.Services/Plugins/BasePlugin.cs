using System.Collections.Generic;

namespace Nop.Services.Plugins
{
    /// <summary>
    /// Base plugin
    /// </summary>
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public virtual string GetConfigurationPageUrl()
        {
            return null;
        }

        /// <summary>
        /// Gets or sets the plugin descriptor
        /// </summary>
        public virtual PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Install plugin
        /// </summary>
        public virtual void Install() 
        {
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public virtual void Uninstall() 
        {
        }

        /// <summary>
        /// Checks if it is possible to uninstall the plugin
        /// </summary>
        /// <param name="installedPluginsAfterRestart">List of plugins that will be active after reboot</param>
        /// <returns>If uninstallation is allowed, NULL or an empty string is returned, otherwise the text of the reason for failure is returned</returns>
        public virtual string CanBeUninstalled(IList<string> installedPluginsAfterRestart)
        {
            return string.Empty;
        }
    }
}
