using CommNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {GameScenes.FLIGHT, GameScenes.TRACKSTATION})]
    public class CNCCommNetScenario : CommNetScenario
    {
        private CNCCommNetUI customUI = null;
        //private CNCCommNetNetwork customNetworkService = null;
        public List<Constellation> constellations;

        public static new CNCCommNetScenario Instance
        {
            get;
            set;
        }

        protected override void Start()
        {
            constellations = new List<Constellation>();
            CNCCommNetScenario.Instance = this;

            CommNetUI ui = FindObjectOfType<CommNetUI>();
            customUI = ui.gameObject.AddComponent<CNCCommNetUI>();
            UnityEngine.Object.Destroy(ui);

            CommNetNetwork.Instance.CommNet = new CNCCommNetwork();
            //customNetworkService = networkService.gameObject.AddComponent<CNCCommNetNetwork>();
            //UnityEngine.Object.Destroy(networkService);

            CommNetHome[] homes = FindObjectsOfType<CommNetHome>();
            for(int i=0; i<homes.Length; i++)
            {
                CNCCommNetHome customHome = homes[i].gameObject.AddComponent(typeof(CNCCommNetHome)) as CNCCommNetHome;
                customHome.copyOf(homes[i]);
                UnityEngine.Object.Destroy(homes[i]);
            }

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
            //if (this.customNetworkService != null)
            //    UnityEngine.Object.Destroy(this.customNetworkService);

            if (this.customUI != null)
                UnityEngine.Object.Destroy(this.customUI);

            GameEvents.OnGameSettingsApplied.Remove(new EventVoid.OnEvent(this.customResetNetwork));
        }

        public void customResetNetwork()
        {
            CNCLog.Debug("CNCCommNetScenario.customResetNetwork()");
            CommNetNetwork.Instance.CommNet = new CNCCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            if (gameNode.HasNode("Constellations"))
            {
                ConfigNode rootNode = gameNode.GetNode("Constellations");
                ConfigNode[] constellationNodes = rootNode.GetNodes();

                for (int i = 0; i < constellationNodes.Length; i++)
                {
                    ConfigNode thisNode = constellationNodes[i];
                    Constellation newConstellation = new Constellation(short.Parse(thisNode.GetValue("frequency")),
                                                                        thisNode.GetValue("name"), 
                                                                        Constellation.parseColor(thisNode.GetValue("color")));
                    constellations.Add(newConstellation);
                }
            }
            else
            {
                constellations = CNCSettings.Instance.Constellations;
            }

        }

        public override void OnSave(ConfigNode gameNode)
        {
            ConfigNode rootNode;

            if (gameNode.HasNode("Constellations"))
            {
                rootNode = gameNode.GetNode("Constellations");
                rootNode.ClearNodes();
            }
            else
            {
                rootNode = new ConfigNode("Constellations");
                gameNode.AddNode(rootNode);
            }

            for (int i=0; i<constellations.Count; i++)
            {
                ConfigNode newConstellationNode = new ConfigNode("Constellation");
                newConstellationNode = ConfigNode.CreateConfigFromObject(constellations.ElementAt<Constellation>(i), newConstellationNode);
                rootNode.AddNode(newConstellationNode);
            }   

            base.OnSave(gameNode);
        }
    }
}
