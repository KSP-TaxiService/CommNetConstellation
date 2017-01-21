using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommNetConstellation
{
    public class CNCSettings
    {
        //Note: This can be called by a PartModule during KSP's Squad-monkey loading screen
        private static Settings privateInstance = null;
        public static Settings Instance
        {
            get
            {
                if (privateInstance is Settings && privateInstance.SettingsLoaded)
                    return privateInstance;
                else
                    return privateInstance = Settings.Load();
            }
        }
    }

    public class Settings
    {
        public bool SettingsLoaded = false;
        private static string startingSettingCFGUrl = "CommNetConstellation/cnc_settings/CommNetConstellationSettings";

        //Global settings to be read from the setting cfg
        //-----
        [Persistent] public int MajorVersion;
        [Persistent] public int MinorVersion;
        [Persistent] public short PublicRadioFrequency;
        [Persistent] public string DefaultPublicName;
        [Persistent] public Color DefaultPublicColor;
        [Persistent] public float DistanceToHideGroundStations;
        [Persistent(collectionIndex = "Constellations")] public List<Constellation> Constellations;
        //-----

        public int MaxNumChars = 23;

        public static Settings Load()
        {
            // Create a blank object of settings
            Settings settings = new Settings();
            bool defaultSuccess = false;

            // Exploit KSP's GameDatabase to find our MM-patched cfg of default settings (from GameData/RemoteTech/Default_Settings.cfg)
            UrlDir.UrlConfig[] cfgs = GameDatabase.Instance.GetConfigs("CommNetConstellationSettings");
            for (var i = 0; i < cfgs.Length; i++)
            {
                if (cfgs[i].url.Equals(startingSettingCFGUrl))
                {
                    defaultSuccess = ConfigNode.LoadObjectFromConfig(settings, cfgs[i].config);
                    CNCLog.Verbose("Load starting settings into object with {0}: LOADED {1}", cfgs[i].config, defaultSuccess ? "OK" : "FAIL");
                    break;
                }
            }

            if (!defaultSuccess) // disable itself and write explanation to KSP's log
            {
                CNCLog.Error("The CommNet Constellation setting file '{0}' is not found!", startingSettingCFGUrl);
                return null;
                // the main impact of returning null is the endless loop of invoking Load() in the KSP's loading screen
            }

            settings.SettingsLoaded = true;
            return settings;
        }
    }
}
