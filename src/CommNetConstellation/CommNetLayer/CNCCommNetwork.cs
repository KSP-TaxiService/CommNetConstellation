using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetwork : CommNetwork
    {
        public CNCCommNetwork() : base()
        {
            CNCLog.Debug("CNCCommNetwork.CNCCommNetwork()");
        }

        public override bool FindClosestControlSource(CommNode from, CommPath path = null)
        {
            CNCLog.Debug("CNCCommNetwork.FindClosestControlSource() : {0}",from.name);
            return base.FindClosestControlSource(from, path);
        }

        public override bool FindHome(CommNode from, CommPath path = null)
        {
            CNCLog.Debug("CNCCommNetwork.FindHome() : {0}", from.name);
            return base.FindHome(from, path);
        }

        protected override bool TryConnect(CommNode a, CommNode b, double distance, bool aCanRelay, bool bCanRelay, bool bothRelay)
        {
            CNCLog.Debug("CNCCommNetwork.TryConnect() : {0} {1}", a.name, b.name);
            return base.TryConnect(a, b, distance, aCanRelay, bCanRelay, bothRelay);
        }
    }
}
