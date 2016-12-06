using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommPath : CommPath
    {
        public override void Clear()
        {
            CNCLog.Debug("CNCCommPath.Clear()");
            base.Clear();
        }
    }
}
