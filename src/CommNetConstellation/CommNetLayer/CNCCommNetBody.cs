using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Copy required for the customised CommNet
    /// </summary>
    public class CNCCommNetBody : CommNetBody
    {
        public void copyOf(CommNetBody stockBody)
        {
            CNCLog.Verbose("CommNet Body '{0}' added", stockBody.name);

            this.body = stockBody.GetComponentInChildren<CelestialBody>();

            //this.occluder is initalised by OnNetworkInitialized() later
        }
    }
}
