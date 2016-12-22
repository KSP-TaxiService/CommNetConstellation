using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {GameScenes.FLIGHT, GameScenes.TRACKSTATION})]
    public class CNCCommNetScenario : CommNetScenario
    {
        private CNCCommNetUI customUI = null;
        private CNCCommNetNetwork customNetworkService = null;

        protected override void Start()
        {
            CommNetUI ui = FindObjectOfType<CommNetUI>();
            customUI = ui.gameObject.AddComponent<CNCCommNetUI>();
            UnityEngine.Object.Destroy(ui);

            CommNetNetwork networkService = FindObjectOfType<CommNetNetwork>();
            customNetworkService = networkService.gameObject.AddComponent<CNCCommNetNetwork>();
            //networkService = customNetworkService; // substitute CNCCommNetNetwork for CommNetNetwork; do not destroy CommNetNetwork because the other classes invoke CommNetNetwork.Add/Remove(...) //TODO what is effect?
            UnityEngine.Object.Destroy(networkService);

            CommNetHome[] homes = FindObjectsOfType<CommNetHome>();
            for(int i=0; i<homes.Length; i++)
            {
                CommNetHome home = homes[i];
                home.gameObject.AddComponent<CNCCommNetHome>();
                UnityEngine.Object.Destroy(homes[i]);
            }

            CommNetBody[] bodies = FindObjectsOfType<CommNetBody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                CommNetBody body = bodies[i];
                body.gameObject.AddComponent<CNCCommNetBody>();
                UnityEngine.Object.Destroy(bodies[i]);
            }

            CommNetNode[] nodes = FindObjectsOfType<CommNetNode>();
            for (int i = 0; i < nodes.Length; i++)
            {
                CommNetNode node = nodes[i];
                node.gameObject.AddComponent<CNCCommNetNode>();
                UnityEngine.Object.Destroy(nodes[i]);
            }
        }

        //override to turn off CommNetScenario's instance check
        public override void OnAwake() { }

        private void OnDestroy()
        {
            if (this.customNetworkService != null)
                UnityEngine.Object.Destroy(this.customNetworkService);

            if (this.customUI != null)
                UnityEngine.Object.Destroy(this.customUI);
        }
    }
}
