using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetNode : CommNetNode
    {
        protected override void OnNetworkInitialized()
        {
            CNCLog.Debug("CNCCommNetNode.OnNetworkInitialized() @ {0}", this.Comm.name);
            base.OnNetworkInitialized();
        }

        public override void OnNetworkPostUpdate()
        {
            CNCLog.Debug("CNCCommNetNode.OnNetworkPostUpdate()");
            base.OnNetworkPostUpdate();
        }

        public override void OnNetworkPreUpdate()
        {
            CNCLog.Debug("CNCCommNetNode.OnNetworkPostUpdate()");
            base.OnNetworkPreUpdate();
        }
    }
}
