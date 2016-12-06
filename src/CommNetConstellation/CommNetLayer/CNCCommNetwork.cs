using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    //[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CNCCommNetwork : CommNetwork
    {
        public CNCCommNetwork(): base()
        {
            CNCLog.Debug("CNCCommNetwork.CNCCommNetwork()");
        }

        public override bool FindClosestControlSource(CommNode from, CommPath path = null)
        {
            CNCLog.Debug("CNCCommNetwork.FindClosestControlSource() from {1}",from.name);
            return base.FindClosestControlSource(from, path);
        }
    }
}
