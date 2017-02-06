using CommNet;
using System.Collections.Generic;
using System.Linq;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// This class is the key that allows to break into and customise KSP's CommNet. This is possibly the secondary model in the Model–view–controller sense
    /// </summary>
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER })]
    public class CNCCommNetScenario : CommNetScenario
    {
        /* Note:
         * 1) On entering a desired scene, OnLoad() and then Start() are called.
         * 2) On leaving the scene, OnSave() is called
         * 3) GameScenes.SPACECENTER is recommended so that the constellation data can be verified and error-corrected in advance
         */

        private CNCCommNetUI CustomCommNetUI = null;
        public List<Constellation> constellations; // leave  the initialisation to OnLoad()

        public static new CNCCommNetScenario Instance
        {
            get;
            set;
        }

        protected override void Start()
        {
            CNCCommNetScenario.Instance = this;
            
            //Steal the CommNet user interface
            CommNetUI ui = FindObjectOfType<CommNetUI>();
            CustomCommNetUI = ui.gameObject.AddComponent<CNCCommNetUI>();
            UnityEngine.Object.Destroy(ui);

            //Steal the CommNet service
            CommNetNetwork.Instance.CommNet = new CNCCommNetwork();

            //Steal the CommNet ground stations
            CommNetHome[] homes = FindObjectsOfType<CommNetHome>();
            for(int i=0; i<homes.Length; i++)
            {
                CNCCommNetHome customHome = homes[i].gameObject.AddComponent(typeof(CNCCommNetHome)) as CNCCommNetHome;
                customHome.copyOf(homes[i]);
                UnityEngine.Object.Destroy(homes[i]);
            }

            //Steal the CommNet celestial bodies
            CommNetBody[] bodies = FindObjectsOfType<CommNetBody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                CNCCommNetBody customBody = bodies[i].gameObject.AddComponent(typeof(CNCCommNetBody)) as CNCCommNetBody;
                customBody.copyOf(bodies[i]);
                UnityEngine.Object.Destroy(bodies[i]);
            }
        }

        public override void OnAwake()
        {
            //override to turn off CommNetScenario's instance check

            GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(this.customResetNetwork));
        }

        private void OnDestroy()
        {
            if (this.CustomCommNetUI != null)
                UnityEngine.Object.Destroy(this.CustomCommNetUI);

            GameEvents.OnGameSettingsApplied.Remove(new EventVoid.OnEvent(this.customResetNetwork));
        }

        public void customResetNetwork()
        {
            CommNetNetwork.Instance.CommNet = new CNCCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
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
        /// Obtain all communicable vessels that has the given frequency
        /// </summary>
        public List<CNCCommNetVessel> getCommNetVessels(short targetFrequency = -1) // TODO: Cache it (maybe dict) and use GameEvents to remove and add?
        {
            List<Vessel> vessels = FlightGlobals.fetch.vessels;
            List<CNCCommNetVessel> commnetVessels = new List<CNCCommNetVessel>();

            for (int i = 0; i < vessels.Count; i++)
            {
                Vessel thisVessel = vessels[i];
                if (thisVessel.Connection != null)
                {
                    CNCCommNetVessel cncVessel = (CNCCommNetVessel)thisVessel.Connection;
                    if (cncVessel.getRadioFrequency() == targetFrequency || targetFrequency == -1)
                    {
                        commnetVessels.Add(cncVessel);
                    }
                }
            }

            return commnetVessels;
        }

        /// <summary>
        /// Find the vessel that has the given comm node
        /// </summary>
        public Vessel findCorrespondingVessel(CommNode commNode)
        {
            List<Vessel> allVessels = FlightGlobals.fetch.vessels;
            IEqualityComparer<CommNode> comparer = commNode.Comparer;

            //brute-force search temporarily until I find a \omega(n) method //TODO: switch to cache (maybe dict)
            for (int i = 0; i < allVessels.Count(); i++)
            {
                Vessel thisVessel = allVessels[i];
                if (thisVessel.connection != null)
                {
                    if (comparer.Equals(commNode, thisVessel.connection.Comm))
                    {
                        return thisVessel;
                    }
                }
            }

            //not found
            return null;
        }
    }
}
