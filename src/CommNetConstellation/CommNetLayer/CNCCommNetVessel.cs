using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetVessel : CommNetVessel
    {
        public short radioFrequency;

        protected override void OnNetworkInitialized()
        {
            CNCLog.Debug("CNCCommNetVessel.OnNetworkInitialized() @ {0}", this.Vessel.GetName());
            this.radioFrequency = CNCSettings.everyoneRadioFrequency;
            base.OnNetworkInitialized();
        }

        public override void OnNetworkPostUpdate()
        {
            //CNPLog.Debug("OnNetworkPostUpdate");
            base.OnNetworkPostUpdate();
        }

        public override void OnNetworkPreUpdate()
        {
            //CNPLog.Debug("OnNetworkPreUpdate");
            base.OnNetworkPreUpdate();
        }

        protected override void UpdateComm()
        {
            //CNPLog.Debug("UpdateComm");
            base.UpdateComm();
        }
    }
}
