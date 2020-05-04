using CommNetConstellation.CommNetLayer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommNetConstellation
{
    /// <summary>
    /// Expose this class to the public for consumption
    /// </summary>
    public class CNCSettings
    {
        public static readonly int MaxLengthName = 25;
        public static readonly int MaxDigits = 5;
        public static readonly float ScreenMessageDuration = 5f;

        //Note: This can be called by a PartModule during KSP's Squad-monkey loading screen
        //so don't be surprised if the post-loading logging does not have the setting verbose
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

    /// <summary>
    /// Data structure to be populated from the setting cfg (obtained from KSP's GameDatabase; can be patched via MM)
    /// </summary>
    public class Settings
    {
        public bool SettingsLoaded = false;
        private static string startingSettingCFGUrl = "";

        public int MajorVersion;
        public int MinorVersion;

        //Global settings
        //-----
        [Persistent] public short PublicRadioFrequency;
        [Persistent] public string DefaultPublicName;
        [Persistent] public Color DefaultPublicColor;
        [Persistent] public float DistanceToHideGroundStations;
        [Persistent] public bool LegacyOrbitLineColor;
        [Persistent(collectionIndex = "Constellation")] public List<Constellation> Constellations;
        [Persistent(collectionIndex = "GroundStation")] public List<CNCCommNetHome> GroundStations;
        [Persistent] private string UpgradeableGroundStationCosts = String.Empty;
        [Persistent] private string UpgradeableGroundStationPowers = String.Empty;
        [Persistent] private string KSCMissionControlPowers = String.Empty;
        //-----

        public int[] GroundStationUpgradeableCosts;
        public double[] GroundStationUpgradeablePowers;
        public double[] KSCStationPowers;

        public void postprocess()
        {
            if (UpgradeableGroundStationCosts != String.Empty)
            {
                var tokens = UpgradeableGroundStationCosts.Split(';');
                GroundStationUpgradeableCosts = new int[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                    int.TryParse(tokens[i], out GroundStationUpgradeableCosts[i]);
                }
            }
            if (UpgradeableGroundStationPowers != String.Empty)
            {
                var tokens = UpgradeableGroundStationPowers.Split(';');
                GroundStationUpgradeablePowers = new double[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                    double.TryParse(tokens[i], out GroundStationUpgradeablePowers[i]);
                }
            }
            if (KSCMissionControlPowers != String.Empty)
            {
                var tokens = KSCMissionControlPowers.Split(';');
                KSCStationPowers = new double[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                    double.TryParse(tokens[i], out KSCStationPowers[i]);
                }
            }
        }

        public static Settings Load()
        {
            // Create a blank object of settings
            Settings settings = new Settings();
            bool defaultSuccess = false;

            AssemblyLoader.LoadedAssembly assemblyDLL = null;
            for(int i=0; i <= AssemblyLoader.loadedAssemblies.Count; i++)
            {
                if (AssemblyLoader.loadedAssemblies[i].assembly.GetName().Name.Equals("CommNetConstellation"))
                {
                    assemblyDLL = AssemblyLoader.loadedAssemblies[i];
                    break;
                }
            }

            startingSettingCFGUrl = assemblyDLL.url.Replace("/Plugins", "") + "/cnc_settings/CommNetConstellationSettings";
            settings.MajorVersion = assemblyDLL.versionMajor;
            settings.MinorVersion = assemblyDLL.versionMinor;

            // Exploit KSP's GameDatabase to find our MM-patched cfg of default settings
            UrlDir.UrlConfig[] cfgs = GameDatabase.Instance.GetConfigs("CommNetConstellationSettings");
            for (var i = 0; i < cfgs.Length; i++)
            {
                if (cfgs[i].url.Equals(startingSettingCFGUrl))
                {
                    defaultSuccess = ConfigNode.LoadObjectFromConfig(settings, cfgs[i].config);
                    
                    //Workaround due to LoadObjectFromConfig not auto-populating ground stations for unknown reason
                    settings.GroundStations = new List<CNCCommNetHome>();
                    ConfigNode[] stationNodes = cfgs[i].config.GetNode("GroundStations").GetNodes();
                    for (int j = 0; j < stationNodes.Length; j++)
                    {
                        CNCCommNetHome dummyGroundStation = new CNCCommNetHome();
                        ConfigNode.LoadObjectFromConfig(dummyGroundStation, stationNodes[j]);
                        settings.GroundStations.Add(dummyGroundStation);
                    }

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
            settings.postprocess();
            return settings;
        }
    }
}
