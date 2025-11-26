using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PvPEAK;

public class ConfigurationHandler
{
    private ConfigFile _config;
    public InputAction MenuAction { get; set; }

    public ConfigEntry<int> ConfigItemSafeDistance;

    public int ItemSafeDistance => ConfigItemSafeDistance.Value;


    public ConfigurationHandler(ConfigFile configFile)
    {
        _config = configFile;
        
        Plugin.Logger.LogInfo("ConfigurationHandler initialising");

        ConfigItemSafeDistance = _config.Bind
        (
            "General",
            "Item Safe Distance",
            15,
            "The maximum distance you must be from a campfire to steal another team's items, in world units."
        );

        Plugin.Logger.LogInfo("ConfigurationHandler: Item Safe Distance Loaded: " + ConfigItemSafeDistance.Value);

        Plugin.Logger.LogInfo("ConfigurationHandler initialised");
    }
}