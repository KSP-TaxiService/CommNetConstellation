using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommLink : CommLink
    {
        public override void Set(CommNode a, CommNode b, double cost)
        {
            CNCLog.Debug("CNCCommLink.Set() {0}--{1} with {2}", a.name, b.name, cost);
            base.Set(a, b, cost);
        }

        public override void Set(CommNode a, CommNode b, double distance, double signalStrength)
        {
            CNCLog.Debug("CNCCommLink.Set() {0}--{1} with {2} and {3}", a.name, b.name, distance, signalStrength);
            base.Set(a, b, distance, signalStrength);
        }
    }
}
