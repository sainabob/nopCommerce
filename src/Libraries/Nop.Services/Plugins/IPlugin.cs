using System.Collections.Generic;

namespace Nop.Services.Plugins
{
    /// <summary>
    /// Interface denoting plug-in attributes that are displayed throughout 
    /// the editing interface.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        string GetConfigurationPageUrl();

        /// <summary>
        /// Gets or sets the plugin descriptor
        /// </summary>
        PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Install plugin
        /// </summary>
        void Install();

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        void Uninstall();

        /// <summary>
        /// Checks if it is possible to uninstall the plugin
        /// </summary>
        /// <param name="installedPluginsAfterRestart">List of plugins that will be active after reboot</param>
        /// <returns>If uninstallation is allowed, NULL or an empty string is returned, otherwise the text of the reason for failure is returned</returns>
        string CanBeUninstalled(IList<string> installedPluginsAfterRestart);
    }
}
