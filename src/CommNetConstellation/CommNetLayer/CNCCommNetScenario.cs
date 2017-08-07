using CommNet;
using System.Collections.Generic;
using System.Linq;

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

        private CNCCommNetUI CustomCommNetUI = null;
        private CommNetNetwork CustomCommNetNetwork = null;
        public List<Constellation> constellations; // leave the initialisation to OnLoad()
        private List<CNCCommNetVessel> commVessels;
        private bool dirtyCommNetVesselList;

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
            // Use CommNetCoop's methods:
            CommNetManagerAPI.CommNetManagerChecker.SetCommNetManagerIfAvailable(this, typeof(CNCCommNetNetwork), out CustomCommNetNetwork);
            /*
            CommNetNetwork net = FindObjectOfType<CommNetNetwork>();
            CustomCommNetNetwork = gameObject.AddComponent<CNCCommNetNetwork>();
            UnityEngine.Object.Destroy(net);
            //CommNetNetwork.Instance.GetType().GetMethod("set_Instance").Invoke(CustomCommNetNetwork, null); // reflection to bypass Instance's protected set // don't seem to work
            */

            /*
            //Replace the CommNet ground stations
            CommNetHome[] homes = FindObjectsOfType<CommNetHome>();
            for(int i=0; i<homes.Length; i++)
            {
                CNCCommNetHome customHome = homes[i].gameObject.AddComponent(typeof(CNCCommNetHome)) as CNCCommNetHome;
                customHome.copyOf(homes[i]);
                UnityEngine.Object.Destroy(homes[i]);
            }

            //Replace the CommNet celestial bodies
            CommNetBody[] bodies = FindObjectsOfType<CommNetBody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                CNCCommNetBody customBody = bodies[i].gameObject.AddComponent(typeof(CNCCommNetBody)) as CNCCommNetBody;
                customBody.copyOf(bodies[i]);
                UnityEngine.Object.Destroy(bodies[i]);
            }
            */
            CommNetManagerAPI.AssemblyChecker.SetCommNetTypes();

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

            this.constellations.Clear();
            this.commVessels.Clear();

            GameEvents.onVesselCreate.Remove(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
            GameEvents.onVesselDestroy.Remove(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            CNCLog.Verbose("Scenario content to be read:\n{0}", gameNode);

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

            constellations.OrderBy(i => i.frequency);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            if (constellations.Count < 1)
            {
                CNCLog.Error("The constellation list to save to persistent.sfs is empty!");
                base.OnSave(gameNode);
                return;
            }

            ConfigNode rootNode;
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

            CNCLog.Verbose("Scenario content to be saved:\n{0}", gameNode);
            base.OnSave(gameNode);
        }

        /// <summary>
        /// Obtain all communicable vessels that have the given frequency
        /// </summary>
        public List<CNCCommNetVessel> getCommNetVessels(short targetFrequency = -1)
        {
            cacheCommNetVessels();

            return commVessels.Where(x => x.getRadioFrequency() == targetFrequency || targetFrequency == -1).ToList();
        }

        /// <summary>
        /// Find the vessel that has the given comm node
        /// </summary>
        public Vessel findCorrespondingVessel(CommNode commNode)
        {
            cacheCommNetVessels();

            return commVessels.Find(x => x.CommNetVessel.Comm.precisePosition == commNode.precisePosition).Vessel; // more specific equal
            //IEqualityComparer<CommNode> comparer = commNode.Comparer; // a combination of third-party mods somehow  affects CommNode's IEqualityComparer on two objects
            //return commVessels.Find(x => comparer.Equals(commNode, x.Comm)).Vessel;
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
                if (allVessels[i].connection != null) // && allVessels[i].FindPartModuleImplementing<ModuleCommand>())
                {
                    CNCLog.Debug("Caching CommNetVessel '{0}'", allVessels[i].vesselName);
                    this.commVessels.Add(((CommNetManagerAPI.ModularCommNetVessel)allVessels[i].connection).GetModuleOfType<CNCCommNetVessel>());
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
    }
}
