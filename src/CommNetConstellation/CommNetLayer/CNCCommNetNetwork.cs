using CommNet;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Extend the functionality of the KSP's CommNetNetwork (co-primary model in the Model–view–controller sense; CommNet<> is the other co-primary one)
    /// </summary>
    public class CNCCommNetNetwork : CommNetNetwork
    {
        //Part of inactive network optimisation in CNCCommNetNetwork.Update()
        //private float nextUpdateTime = 0.0f;
        //private const float networkInterval = 0.1f; // in seconds

        protected override void Awake()
        {
            CNCLog.Verbose("CommNet Network booting");

            CommNetNetwork.Instance = this;
            this.CommNet = new CNCCommNetwork();

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                GameEvents.onPlanetariumTargetChanged.Add(new EventData<MapObject>.OnEvent(this.OnMapFocusChange));
            }

            GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(this.ResetNetwork));
            ResetNetwork(); // Please retain this so that KSP can properly reset
        }

        protected new void ResetNetwork()
        {
            CNCLog.Verbose("CommNet Network rebooted");

            this.CommNet = new CNCCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
        }

        protected override void Update()
        {
            //Comment: Not recommended to run along with other active optimisation of evaluating
            //subset of connections in CNCCommNetwork.UpdateNetwork()
            //Effect of running both optimisations is unacceptable low rate of connection check per second
            //if (Time.time >= nextUpdateTime)
            //{
            base.Update();
                //nextUpdateTime += networkInterval;
            //}
        }
    }
}
