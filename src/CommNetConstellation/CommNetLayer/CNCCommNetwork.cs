using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetwork : CommNetwork
    {
        public CNCCommNetwork() : base()
        {
            CNCLog.Debug("CNCCommNetwork()");
        }

        public override CommNode Add(CommNode conn)
        {
            CNCLog.Debug("CNCCommNetwork.Add() : {0}", conn.name);
            return base.Add(conn);
        }

        public override bool Remove(CommNode conn)
        {
            CNCLog.Debug("CNCCommNetwork.Remove() : {0}", conn.name);
            return base.Remove(conn);
        }
    }
}
