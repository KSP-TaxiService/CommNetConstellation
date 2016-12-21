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
