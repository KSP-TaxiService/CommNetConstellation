using UnityEngine;
using CommNet;

namespace CommNetConstellation
{
    //public class CommNetConstellationModule: PartModule

    // Called when you are in the flight scene
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CommNetConstellation : MonoBehaviour
    {
        public void Start()
        {
            CNCLog.Verbose("Flight script starts");
        }

        public void OnDestroy()
        {
            CNCLog.Verbose("Flight script ends");
        }

        public void Awake()
        {
            CNCLog.Verbose("Flight script awakes");
        }

        public void Update()
        {
            //CNCLog.Verbose("Flight script updates");
        }
    }
}
