using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetNetwork : CommNetNetwork
    {
        public static void upgradeToCNCCommNetNetwork()
        {
            CNCLog.Debug("CNCCommNetNetwork.upgradeToCNCCommNetNetwork()");
            CommNetNetwork.Instance = new CNCCommNetNetwork();
        }

        public CNCCommNetNetwork() : base()
        {
            base.CommNet = new CNCCommNetwork();
        }

        public override CommNetwork CommNet
        {
            get
            {
                CNCLog.Debug("CNCCommNetNetwork.CommNet - get");
                return base.CommNet;
            }

            set
            {
                CNCLog.Debug("CNCCommNetNetwork.CommNet - set");
                base.CommNet = value;
            }
        }
    }
}
