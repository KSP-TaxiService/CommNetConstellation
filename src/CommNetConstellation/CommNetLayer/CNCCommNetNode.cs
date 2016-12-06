using CommNet;

// purpose?

namespace CommNetConstellation.CommNetLayer
{
    //[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CNCCommNetNode : CommNetNode
    {
        public override void OnNetworkPostUpdate()
        {
            //CNCLog.Debug("CNCCommNetNode.OnNetworkPostUpdate() : {0}", base.comm.ToString());
            base.OnNetworkPostUpdate();
        }

        public override void OnNetworkPreUpdate()
        {
            //CNCLog.Debug("CNCCommNetNode.OnNetworkPreUpdate() : {0}", base.comm.ToString());
            base.OnNetworkPreUpdate();
        }

        protected override void OnNetworkInitialized()
        {
            CNCLog.Debug("CNCCommNetNode.OnNetworkInitialized() : {0}", base.comm.ToString());
            base.OnNetworkInitialized();
        }
    }
}
