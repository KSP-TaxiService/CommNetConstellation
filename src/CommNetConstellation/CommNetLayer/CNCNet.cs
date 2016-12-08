using CommNet.Network;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCNet<_Net, _Data, _Link, _Path> : Net<_Net, _Data, _Link, _Path>
        where _Net : Net<_Net, _Data, _Link, _Path>
        where _Data : Node<_Net, _Data, _Link, _Path>
        where _Link : Link<_Net, _Data, _Link, _Path>, new()
        where _Path : Path<_Net, _Data, _Link, _Path>
    {
        public CNCNet() : base()
        {
            CNCLog.Debug("CNCNet constructor");
        }

        protected override bool SetNodeConnection(_Data connA, _Data connB)
        {
            CNCLog.Debug("CNCNet.SetNodeConnection()");
            return true;
        }
    }
}
