using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetVessel : CommNetVessel
    {
        [KSPField(isPersistant = true)]
        public short radioFrequency = CNCSettings.everyoneRadioFrequency;

        protected override void OnNetworkInitialized()
        {
            CNCLog.Debug("CNCCommNetVessel.OnNetworkInitialized() @ {0}", this.Vessel.GetName());
            //base.comm = new CNCCommNode(base.comm);
            base.OnNetworkInitialized();
        }

        public override void OnNetworkPostUpdate()
        {
            //CNCLog.Debug("CNCCommNetVessel.OnNetworkPostUpdate()");
            base.OnNetworkPostUpdate();
        }

        public override void OnNetworkPreUpdate()
        {
            //CNCLog.Debug("CNCCommNetVessel.OnNetworkPreUpdate()");
            base.OnNetworkPreUpdate();
        }

        protected override void UpdateComm()
        {
            CNCLog.Debug("CNCCommNetVessel.UpdateComm()");
            base.UpdateComm();
        }
    }
}
