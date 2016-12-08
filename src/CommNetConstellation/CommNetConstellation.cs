using UnityEngine;
using CommNet;
using CommNetConstellation.CommNetLayer;

namespace CommNetConstellation
{
    //public class CommNetConstellationModule: PartModule

    // Called when you are in the flight scene
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CommNetConstellation : MonoBehaviour
    {
        public void Start()
        {
            CNCLog.Debug("CommNetConstellation.Start()");
            //if (!CommNetNetwork.Initialized)
                //CNCCommNetNetwork.upgradeToCNCCommNetNetwork();
        }

        public void OnDestroy()
        {
            
        }

        public void Update()
        {
            return;
            if (CommNetNetwork.Initialized)
            {
                if (CommNetNetwork.Instance.CommNet is CommNetwork)
                    CNCLog.Debug("CommNetConstellation.Update() : CommNetwork type");

                if (CommNetNetwork.Instance.CommNet is CNCCommNetwork)
                    CNCLog.Debug("CommNetConstellation.Update() : CNCCommNetwork type");
            }
        }
    }
}
