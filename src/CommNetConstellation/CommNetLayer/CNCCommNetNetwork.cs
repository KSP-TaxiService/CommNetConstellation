using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Extend the functionality of the KSP's CommNetNetwork (co-primary model in the Model–view–controller sense; CommNet<> is the other co-primary one)
    /// </summary>
    public class CNCCommNetNetwork : CommNetNetwork
    {
        public static new CNCCommNetNetwork Instance
        {
            get;
            protected set;
        }

        protected override void Awake()
        {
            CNCLog.Verbose("CommNet Network booting");

            CommNetNetwork.Instance = this;
            CommNetNetwork.Instance.CommNet = new CNCCommNetwork();

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                GameEvents.onPlanetariumTargetChanged.Add(new EventData<MapObject>.OnEvent(this.OnMapFocusChange));
            }

            GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(this.ResetNetwork));
            CommNetNetwork.Reset(); // Please retain this so that KSP can properly reset
        }

        protected new void ResetNetwork()
        {
            CNCLog.Verbose("CommNet Network rebooted");

            this.CommNet = new CNCCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
        }
    }
}
