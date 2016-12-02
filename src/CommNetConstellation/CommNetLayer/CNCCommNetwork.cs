using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetwork : CommNetwork
    {
        protected override bool TryConnect(CommNode a, CommNode b, double distance, bool aCanRelay, bool bCanRelay, bool bothRelay)
        {
            CNCLog.Debug("CNCCommNetwork.TryConnect() {0}--{1} with {2}", a.name, b.name, distance);
            return base.TryConnect(a, b, distance, aCanRelay, bCanRelay, bothRelay);
        }
    }
}
