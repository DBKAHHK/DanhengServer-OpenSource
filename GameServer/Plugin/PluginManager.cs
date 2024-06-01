using EggLink.DanhengServer.Plugin.Constructor;
using EggLink.DanhengServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Plugin
{
    public class PluginManager
    {
        private readonly static Logger logger = new("PluginManager");
        public readonly static List<IPlugin> Plugins = [];

        public readonly static Dictionary<IPlugin, List<Type>> PluginAssemblies = [];

        public static void LoadPlugins()
        {
            // get all the plugins in the plugin directory
            if (!Directory.Exists(ConfigManager.Config.Path.PluginPath))
            {
                Directory.CreateDirectory(ConfigManager.Config.Path.PluginPath);
            }

            var plugins = Directory.GetFiles(ConfigManager.Config.Path.PluginPath, "*.dll");
            foreach (var plugin in plugins)
            {
                var fileInfo = new FileInfo(plugin);
                LoadPlugin(fileInfo.FullName);
            }

            logger.Info($"Loaded {Plugins.Count} plugins");
        }

        public static void LoadPlugin(string plugin)
        {
            try
            {
                var assembly = Assembly.LoadFile(plugin);
                var types = assembly.GetTypes();

                bool isPlugin = false;
                foreach (var type in types)
                {
                    if (type.GetInterface("IPlugin") != null)
                    {
                        if (Activator.CreateInstance(type) is IPlugin pluginInstance)
                        {
                            pluginInstance.OnLoad();
                            Plugins.Add(pluginInstance);

                            if (!PluginAssemblies.TryGetValue(pluginInstance, out List<Type>? value))
                            {
                                value = new List<Type>();
                                PluginAssemblies[pluginInstance] = value;
                            }

                            value.AddRange(types);

                            isPlugin = true;
                            break;
                        }
                        else
                        {
                            logger.Error($"Failed to load plugin {plugin}: Plugin instance is null");
                        }
                    }
                }

                if (!isPlugin)
                {
                    logger.Error($"Failed to load plugin {plugin}: Plugin does not implement IPlugin");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to load plugin {plugin}: {e.Message}");
            }
        }

        public static void UnloadPlugins()
        {
            foreach (var plugin in Plugins)
            {
                plugin.OnUnload();
            }

            logger.Info("Unloaded all plugins");
            Plugins.Clear();
        }

        public static List<Type> GetPluginAssemblies()
        {
            var assemblies = new List<Type>();
            foreach (var plugin in Plugins)
            {
                if (PluginAssemblies.TryGetValue(plugin, out List<Type>? value))
                {
                    assemblies.AddRange(value);
                }
            }

            return assemblies;
        }
    }
}
