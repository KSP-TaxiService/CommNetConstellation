using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {GameScenes.FLIGHT, GameScenes.TRACKSTATION})]
    public class CNCCommNetScenario : CommNetScenario
    {
        private CNCCommNetUI customUI = null;
        //private CNCCommNetNetwork customNetworkService = null;

        protected override void Start()
        {
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
    }
}
