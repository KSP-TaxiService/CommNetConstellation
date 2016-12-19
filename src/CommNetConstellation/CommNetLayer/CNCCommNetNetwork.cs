using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetNetwork : CommNetNetwork
    {
        public CNCCommNetNetwork() : base()
        {
            CNCLog.Debug("CNCCommNetNetwork()");
            this.CommNet = new CNCCommNetwork();
        }

        public override CommNetwork CommNet
        {
            get
            {
                return this.commNet;
            }
            set
            {
                this.commNet = value;
            }
        }

        // override the method to switch to CNCCommNetwork class
        protected new void ResetNetwork()
        {
            CNCLog.Debug("CNCCommNetNetwork.ResetNetwork()");
            this.commNet = new CNCCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
        }
    }
}
