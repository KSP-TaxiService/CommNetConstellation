using CommNet;
using CommNetConstellation.UI;
using System.Collections.Generic;
using System.Linq;

namespace CommNetConstellation.CommNetLayer
{
    //This class is coupled with the cnc_module.cfg
    public class CNConstellationModule : PartModule
    {
        [KSPField(isPersistant = true)]
        public short radioFrequency = CNCSettings.Instance.PublicRadioFrequency;

        [KSPEvent(guiActive = true, guiActiveEditor =true, guiActiveUnfocused = false, guiName = "CommNet Constellation", active = true)]
        public void KSPEventConstellationSetup()
        {
            new VesselSetupDialog("CommNet Constellation - <color=#00ff00>Setup</color>", this.vessel, this).launch();
        }
    }

    public class CNCCommNetVessel : CommNetVessel
    {
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
            //CNCLog.Debug("CNCCommNetVessel.UpdateComm()");
            base.UpdateComm();
        }

        public short getRadioFrequency()
        {
            List<ProtoPartSnapshot> parts = this.Vessel.protoVessel.protoPartSnapshots;

            for (int i = 0; i < parts.Count; i++)
            {
                ProtoPartSnapshot thisPart = parts.ElementAt(i);
                ProtoPartModuleSnapshot thisModule = thisPart.FindModule("CNConstellationModule");

                if (thisModule == null)
                    continue;

                short radioFreq = short.Parse(thisModule.moduleValues.GetValue("radioFrequency"));
                return radioFreq;
            }

            CNCLog.Error("CommNetVessel '{0}' has no radio frequency!", this.Vessel.GetName());
            return CNCSettings.Instance.PublicRadioFrequency;
        }
    }
}
