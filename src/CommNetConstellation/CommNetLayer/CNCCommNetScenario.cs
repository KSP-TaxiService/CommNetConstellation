using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    public class CNCCommNetScenario : CommNetScenario
    {
        //these variables from the base are private so clone your own variables
        private CNCCommNetNetwork network;
        private CNCCommNetUI ui;

        protected override void Start()
        {
            CNCLog.Debug("CNCCommNetScenario.Start()");
            this.ui = base.gameObject.AddComponent<CNCCommNetUI>();
            this.network = base.gameObject.AddComponent<CNCCommNetNetwork>();
        }

        public override void OnAwake()
        {
            CNCLog.Debug("CNCCommNetScenario.OnAwake()");
        }

        private void OnDestroy()
        {
            CNCLog.Debug("CNCCommNetScenario.OnDestroy()");
            if (this.network != null)
            {
                UnityEngine.Object.Destroy(this.network);
            }

            if (this.ui != null)
            {
                UnityEngine.Object.Destroy(this.ui);
            }
        }
    }
}
