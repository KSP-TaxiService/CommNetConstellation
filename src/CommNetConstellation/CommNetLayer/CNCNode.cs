using CommNet.Network;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCNode<_Net, _Data, _Link, _Path> : Node<_Net, _Data, _Link, _Path>
        where _Net : Net<_Net, _Data, _Link, _Path>
        where _Data : Node<_Net, _Data, _Link, _Path>
        where _Link : Link<_Net, _Data, _Link, _Path>, new()
        where _Path : Path<_Net, _Data, _Link, _Path>
    {
        public CNCNode() : base()
        {
            CNCLog.Debug("CNCNode constructor");
        }
    }
}
