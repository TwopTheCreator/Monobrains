using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class PluginManager : IPluginManager
    {
        private readonly IPythonBackendService _pythonBackend;
        private readonly Dictionary<string, PluginInfo> _loadedPlugins;
        private readonly string _pluginDirectory;

        public PluginManager(IPythonBackendService pythonBackend)
        {
            _pythonBackend = pythonBackend;
            _loadedPlugins = new Dictionary<string, PluginInfo>();
            _pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            
            if (!Directory.Exists(_pluginDirectory))
            {
                Directory.CreateDirectory(_pluginDirectory);
            }
        }

        public async Task<bool> LoadPluginAsync(string pluginPath)
        {
            try
            {
                var success = await _pythonBackend.LoadPluginAsync(pluginPath);
                if (success)
                {
                    var pluginInfo = new PluginInfo
                    {
                        Name = Path.GetFileNameWithoutExtension(pluginPath),
                        Version = "1.0.0",
                        Description = "Plugin loaded from " + pluginPath,
                        Author = "Unknown",
                        Dependencies = new List<string>(),
                        IsLoaded = true
                    };
                    
                    _loadedPlugins[pluginInfo.Name] = pluginInfo;
                }
                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UnloadPluginAsync(string pluginName)
        {
            try
            {
                var success = await _pythonBackend.UnloadPluginAsync(pluginName);
                if (success && _loadedPlugins.ContainsKey(pluginName))
                {
                    _loadedPlugins.Remove(pluginName);
                }
                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<PluginInfo>> GetLoadedPluginsAsync()
        {
            try
            {
                var plugins = await _pythonBackend.GetLoadedPluginsAsync();
                return plugins;
            }
            catch (Exception)
            {
                return new List<PluginInfo>();
            }
        }

        public async Task<List<PluginInfo>> GetAvailablePluginsAsync()
        {
            var plugins = new List<PluginInfo>();
            
            if (Directory.Exists(_pluginDirectory))
            {
                var pluginFiles = Directory.GetFiles(_pluginDirectory, "*.dll");
                
                foreach (var pluginFile in pluginFiles)
                {
                    try
                    {
                        var pluginInfo = new PluginInfo
                        {
                            Name = Path.GetFileNameWithoutExtension(pluginFile),
                            Version = "1.0.0",
                            Description = "Available plugin",
                            Author = "Unknown",
                            Dependencies = new List<string>(),
                            IsLoaded = _loadedPlugins.ContainsKey(Path.GetFileNameWithoutExtension(pluginFile))
                        };
                        
                        plugins.Add(pluginInfo);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            
            return plugins;
        }

        public async Task<bool> InstallPluginAsync(string pluginName, string version)
        {
            return false;
        }

        public async Task<bool> UninstallPluginAsync(string pluginName)
        {
            return false;
        }

        public async Task<PluginConfiguration> GetPluginConfigurationAsync(string pluginName)
        {
            return new PluginConfiguration
            {
                PluginName = pluginName,
                Settings = new Dictionary<string, object>()
            };
        }

        public async Task<bool> SetPluginConfigurationAsync(string pluginName, PluginConfiguration config)
        {
            return true;
        }

        public void LoadPlugins()
        {
            if (Directory.Exists(_pluginDirectory))
            {
                var pluginFiles = Directory.GetFiles(_pluginDirectory, "*.py");
                
                foreach (var pluginFile in pluginFiles)
                {
                    try
                    {
                        LoadPluginAsync(pluginFile).Wait();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }
    }

    public class PluginConfiguration
    {
        public string PluginName { get; set; }
        public Dictionary<string, object> Settings { get; set; }
    }
}
