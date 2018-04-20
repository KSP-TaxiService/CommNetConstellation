using CommNet;
using KSP.UI.Screens.Flight;
using System;
using System.Collections.Generic;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// This class is the key that allows to break into and customise KSP's CommNet. This is possibly the secondary model in the Model–view–controller sense
    /// </summary>
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR })]
    public class CNCCommNetScenario : CommNetScenario
    {
        /* Note:
         * 1) On entering a desired scene, OnLoad() and then Start() are called.
         * 2) On leaving the scene, OnSave() is called
         * 3) GameScenes.SPACECENTER is recommended so that the constellation data can be verified and error-corrected in advance
         */

        //TODO: investigate to add extra ground stations to the group of existing stations

        private CNCCommNetUI CustomCommNetUI = null;
        private CNCCommNetNetwork CustomCommNetNetwork = null;
        private CNCTelemetryUpdate CustomCommNetTelemetry = null;
        private CNCCommNetUIModeButton CustomCommNetModeButton = null;
        public List<Constellation> constellations; // leave the initialisation to OnLoad()
        public List<CNCCommNetHome> groundStations; // leave the initialisation to OnLoad()
        private List<CNCCommNetHome> persistentGroundStations; // leave the initialisation to OnLoad()
        private List<CNCCommNetVessel> commVessels;
        private bool dirtyCommNetVesselList;
        public bool hideGroundStations;

        public static new CNCCommNetScenario Instance
        {
            get;
            protected set;
        }

        protected override void Start()
        {
            CNCCommNetScenario.Instance = this;
            this.commVessels = new List<CNCCommNetVessel>();
            this.dirtyCommNetVesselList = true;

            CNCLog.Verbose("CommNet Scenario loading ...");

            //Replace the CommNet user interface
            CommNetUI ui = FindObjectOfType<CommNetUI>(); // the order of the three lines is important
            CustomCommNetUI = gameObject.AddComponent<CNCCommNetUI>(); // gameObject.AddComponent<>() is "new" keyword for Monohebaviour class
            UnityEngine.Object.Destroy(ui);

            //Replace the CommNet network
            CommNetNetwork net = FindObjectOfType<CommNetNetwork>();
            CustomCommNetNetwork = gameObject.AddComponent<CNCCommNetNetwork>();
            UnityEngine.Object.Destroy(net);
            //CommNetNetwork.Instance.GetType().GetMethod("set_Instance").Invoke(CustomCommNetNetwork, null); // reflection to bypass Instance's protected set // don't seem to work

            //Replace the TelemetryUpdate
            TelemetryUpdate tel = TelemetryUpdate.Instance; //only appear in flight
            CommNetUIModeButton cnmodeUI = FindObjectOfType<CommNetUIModeButton>(); //only appear in tracking station; initialised separately by TelemetryUpdate in flight
            if (tel != null && HighLogic.LoadedSceneIsFlight)
            {
                TelemetryUpdateData tempData = new TelemetryUpdateData(tel);
                UnityEngine.Object.DestroyImmediate(tel); //seem like UE won't initialise CNCTelemetryUpdate instance in presence of TelemetryUpdate instance
                CustomCommNetTelemetry = gameObject.AddComponent<CNCTelemetryUpdate>();
                CustomCommNetTelemetry.copyOf(tempData);
            }
            else if(cnmodeUI != null && HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                CustomCommNetModeButton = cnmodeUI.gameObject.AddComponent<CNCCommNetUIModeButton>();
                CustomCommNetModeButton.copyOf(cnmodeUI);
                UnityEngine.Object.DestroyImmediate(cnmodeUI);
            }

            //Replace the CommNet ground stations
            groundStations = new List<CNCCommNetHome>();
            CommNetHome[] homes = FindObjectsOfType<CommNetHome>();
            for(int i=0; i<homes.Length; i++)
            {
                CNCCommNetHome customHome = homes[i].gameObject.AddComponent(typeof(CNCCommNetHome)) as CNCCommNetHome;
                customHome.copyOf(homes[i]);
                UnityEngine.Object.Destroy(homes[i]);
                groundStations.Add(customHome);
            }
            groundStations.Sort();

            //Apply the ground-station changes from persistent.sfs
            for (int i=0; i<persistentGroundStations.Count;i++)
            {
                if(groundStations.Exists(x => x.ID.Equals(persistentGroundStations[i].ID)))
                {
                    groundStations.Find(x => x.ID.Equals(persistentGroundStations[i].ID)).applySavedChanges(persistentGroundStations[i]);
                }
            }
            persistentGroundStations.Clear();//dont need anymore

            //Replace the CommNet celestial bodies
            CommNetBody[] bodies = FindObjectsOfType<CommNetBody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                CNCCommNetBody customBody = bodies[i].gameObject.AddComponent(typeof(CNCCommNetBody)) as CNCCommNetBody;
                customBody.copyOf(bodies[i]);
                UnityEngine.Object.Destroy(bodies[i]);
            }

            CNCLog.Verbose("CommNet Scenario loading done! ");
        }

        public override void OnAwake()
        {
            //override to turn off CommNetScenario's instance check

            GameEvents.onVesselCreate.Add(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
            GameEvents.onVesselDestroy.Add(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
        }

        private void OnDestroy()
        {
            if (this.CustomCommNetUI != null)
                UnityEngine.Object.Destroy(this.CustomCommNetUI);

            if (this.CustomCommNetNetwork != null)
                UnityEngine.Object.Destroy(this.CustomCommNetNetwork);

            if (this.CustomCommNetTelemetry != null)
                UnityEngine.Object.Destroy(this.CustomCommNetTelemetry);

            if (this.CustomCommNetModeButton != null)
                UnityEngine.Object.Destroy(this.CustomCommNetModeButton);

            this.constellations.Clear();
            this.commVessels.Clear();
            this.groundStations.Clear();

            GameEvents.onVesselCreate.Remove(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
            GameEvents.onVesselDestroy.Remove(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            CNCLog.Verbose("Scenario content to be read:\n{0}", gameNode);

            //Other variables
            for (int i = 0; i < gameNode.values.Count; i++)
            {
                ConfigNode.Value value = gameNode.values[i];
                string name = value.name;
                switch (name)
                {
                    case "DisplayModeTracking":
                        CNCCommNetUI.CustomModeTrackingStation = (CNCCommNetUI.CustomDisplayMode)((int)Enum.Parse(typeof(CNCCommNetUI.CustomDisplayMode), value.value));
                        break;
                    case "DisplayModeFlight":
                        CNCCommNetUI.CustomModeFlightMap = (CNCCommNetUI.CustomDisplayMode)((int)Enum.Parse(typeof(CNCCommNetUI.CustomDisplayMode), value.value));
                        break;
                    case "HideGroundStations":
                        this.hideGroundStations = Boolean.Parse(value.value);
                        break;
                }
            }

            //Constellations
            if (gameNode.HasNode("Constellations"))
            {
                ConfigNode rootNode = gameNode.GetNode("Constellations");
                ConfigNode[] constellationNodes = rootNode.GetNodes();

                if (constellationNodes.Length < 1) // missing constellation list
                {
                    CNCLog.Error("The 'Constellations' node is malformed! Reverted to the default constellation list.");
                    constellations = CNCSettings.Instance.Constellations;
                }
                else
                {
                    constellations = new List<Constellation>();

                    for (int i = 0; i < constellationNodes.Length; i++)
                    {
                        Constellation newConstellation = new Constellation();
                        ConfigNode.LoadObjectFromConfig(newConstellation, constellationNodes[i]);
                        constellations.Add(newConstellation);
                    }
                    ConfigNode.LoadObjectFromConfig(this, rootNode);
                }
            }
            else
            {
                CNCLog.Verbose("The 'Constellations' node is not found. The default constellation list is loaded.");
                constellations = CNCSettings.Instance.Constellations;
            }

            constellations.Sort();

            //Ground stations
            persistentGroundStations = new List<CNCCommNetHome>();
            if (gameNode.HasNode("GroundStations"))
            {
                ConfigNode rootNode = gameNode.GetNode("GroundStations");
                ConfigNode[] stationNodes = rootNode.GetNodes();

                if (stationNodes.Length < 1) // missing ground-station list
                {
                    CNCLog.Error("The 'GroundStations' node is malformed! Reverted to the default list of ground stations.");
                    //do nothing since KSP provides this default list
                }
                else
                {
                    for (int i = 0; i < stationNodes.Length; i++)
                    {
                        CNCCommNetHome dummyGroundStation = new CNCCommNetHome();
                        ConfigNode.LoadObjectFromConfig(dummyGroundStation, stationNodes[i]);
                        persistentGroundStations.Add(dummyGroundStation);

                        if(!stationNodes[i].HasNode("Frequencies")) // empty list is not saved as empty node in persistent.sfs
                        {
                            dummyGroundStation.Frequencies.Clear();// clear the default frequency list
                        }
                    }
                    ConfigNode.LoadObjectFromConfig(this, rootNode);
                }
            }
            else
            {
                CNCLog.Verbose("The 'GroundStations' node is not found. The default list of ground stations is loaded from KSP's data.");
                //do nothing since KSP provides this default list
            }
        }

        public override void OnSave(ConfigNode gameNode)
        {
            ConfigNode rootNode;

            //Other variables
            gameNode.AddValue("DisplayModeTracking", CNCCommNetUI.CustomModeTrackingStation);
            gameNode.AddValue("DisplayModeFlight", CNCCommNetUI.CustomModeFlightMap);
            gameNode.AddValue("HideGroundStations", this.hideGroundStations);

            //Constellations
            if (!gameNode.HasNode("Constellations"))
            {
                rootNode = new ConfigNode("Constellations");
                gameNode.AddNode(rootNode);
            }
            else
            {
                rootNode = gameNode.GetNode("Constellations");
                rootNode.ClearNodes();
            }

            for (int i=0; i<constellations.Count; i++)
            {
                ConfigNode newConstellationNode = new ConfigNode("Constellation");
                newConstellationNode = ConfigNode.CreateConfigFromObject(constellations[i], newConstellationNode);
                rootNode.AddNode(newConstellationNode);
            }

            //Ground stations
            if (!gameNode.HasNode("GroundStations"))
            {
                rootNode = new ConfigNode("GroundStations");
                gameNode.AddNode(rootNode);
            }
            else
            {
                rootNode = gameNode.GetNode("GroundStations");
                rootNode.ClearNodes();
            }

            for (int i = 0; i < groundStations.Count; i++)
            {
                ConfigNode newGroundStationNode = new ConfigNode("GroundStation");
                newGroundStationNode = ConfigNode.CreateConfigFromObject(groundStations[i], newGroundStationNode);
                rootNode.AddNode(newGroundStationNode);
            }

            CNCLog.Verbose("Scenario content to be saved:\n{0}", gameNode);
            base.OnSave(gameNode);
        }

        /// <summary>
        /// Obtain all communicable vessels that have the given frequency
        /// </summary>
        public List<CNCCommNetVessel> getCommNetVessels(short targetFrequency = -1)
        {
            cacheCommNetVessels();

            List<CNCCommNetVessel> newList = new List<CNCCommNetVessel>();
            for(int i=0; i<commVessels.Count; i++)
            {
                if (commVessels[i].getFrequencies().Contains(targetFrequency) || targetFrequency == -1)
                    newList.Add(commVessels[i]);
            }

            return newList;
        }

        /// <summary>
        /// Find the vessel that has the given comm node
        /// </summary>
        public Vessel findCorrespondingVessel(CommNode commNode)
        {
            cacheCommNetVessels();

            return commVessels.Find(x => CNCCommNetwork.AreSame(x.Comm, commNode)).Vessel; // more specific equal
            //IEqualityComparer<CommNode> comparer = commNode.Comparer; // a combination of third-party mods somehow  affects CommNode's IEqualityComparer on two objects
            //return commVessels.Find(x => comparer.Equals(commNode, x.Comm)).Vessel;
        }

        /// <summary>
        /// Find the ground station that has the given comm node
        /// </summary>
        public CNCCommNetHome findCorrespondingGroundStation(CommNode commNode)
        {
            return groundStations.Find(x => CNCCommNetwork.AreSame(x.commNode, commNode));
        }

        /// <summary>
        /// Cache eligible vessels of the FlightGlobals
        /// </summary>
        private void cacheCommNetVessels()
        {
            if (!this.dirtyCommNetVesselList)
                return;

            CNCLog.Verbose("CommNetVessel cache - {0} entries deleted", this.commVessels.Count);
            this.commVessels.Clear();

            List<Vessel> allVessels = FlightGlobals.fetch.vessels;
            for (int i = 0; i < allVessels.Count; i++)
            {
                if (allVessels[i].connection != null && (allVessels[i].connection as CNCCommNetVessel).IsCommandable && allVessels[i].vesselType != VesselType.Unknown)// && allVessels[i].vesselType != VesselType.Debris) // debris could be spent stage with functional probes and antennas
                {
                    CNCLog.Debug("Caching CommNetVessel '{0}'", allVessels[i].vesselName);
                    this.commVessels.Add(allVessels[i].connection as CNCCommNetVessel);
                }
            }

            CNCLog.Verbose("CommNetVessel cache - {0} entries added", this.commVessels.Count);

            this.dirtyCommNetVesselList = false;
        }

        /// <summary>
        /// GameEvent call for newly-created vessels (launch, staging, new asteriod etc)
        /// NOTE: Vessel v is fresh bread straight from the oven before any curation is done on this (i.e. debris.Connection is valid)
        /// </summary>
        private void onVesselCountChanged(Vessel v)
        {
            if (v.vesselType == VesselType.Base || v.vesselType == VesselType.Lander || v.vesselType == VesselType.Plane ||
               v.vesselType == VesselType.Probe || v.vesselType == VesselType.Relay || v.vesselType == VesselType.Rover ||
               v.vesselType == VesselType.Ship || v.vesselType == VesselType.Station)
            {
                CNCLog.Debug("Change in the vessel list detected. Cache refresh required.");
                this.dirtyCommNetVesselList = true;
            }
        }

        /// <summary>
        /// Convenient method to obtain a frequency list from a given CommNode
        /// </summary>
        public List<short> getFrequencies(CommNode a)
        {
            List<short> aFreqs = new List<short>();

            if (a.isHome && findCorrespondingGroundStation(a) != null)
            {
                aFreqs.AddRange(findCorrespondingGroundStation(a).Frequencies);
            }
            else
            {
                aFreqs = ((CNCCommNetVessel)findCorrespondingVessel(a).Connection).getFrequencies();
            }

            return aFreqs;
        }

        /// <summary>
        /// Convenient method to obtain Comm Power of given frequency from a given CommNode
        /// </summary>
        public double getCommPower(CommNode a, short frequency)
        {
            double power = 0.0;

            if (a.isHome && findCorrespondingGroundStation(a) != null)
            {
                power = a.antennaRelay.power;
            }
            else
            {
                power = ((CNCCommNetVessel)findCorrespondingVessel(a).Connection).getMaxComPower(frequency);
            }

            return power;
        }
    }
}
