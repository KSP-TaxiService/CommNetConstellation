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
            new VesselSetupDialog("Vessel - <color=#00ff00>Setup</color>", this.vessel, this).launch(new System.Object[] { this.vessel });
        }
    }

    public class CNCCommNetVessel : CommNetVessel
    {
        protected short radioFrequency;

        protected override void OnNetworkInitialized()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;

            //CNCLog.Debug("CNCCommNetVessel.OnNetworkInitialized() @ {0}", this.Vessel.GetName());

            base.OnNetworkInitialized();
            this.radioFrequency = getRadioFrequency(true);
        }

        public override void OnNetworkPostUpdate()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;

            //CNCLog.Debug("CNCCommNetVessel.OnNetworkPostUpdate()");
            base.OnNetworkPostUpdate();
        }

        public override void OnNetworkPreUpdate()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;

            //CNCLog.Debug("CNCCommNetVessel.OnNetworkPreUpdate()");
            base.OnNetworkPreUpdate();
        }

        protected override void UpdateComm()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;

            //CNCLog.Debug("CNCCommNetVessel.UpdateComm()");
            base.UpdateComm();
        }

        public void updateRadioFrequency(short newFrequency)
        {
            if (!Constellation.isFrequencyValid(newFrequency))
            {
                CNCLog.Error("The new frequency {0} is out of the range [0,{1}]!", newFrequency, short.MaxValue);
                return;
            }

            bool success = false;
            if (this.Vessel.loaded)
            {
                CNConstellationModule thisModule = this.Vessel.FindPartModuleImplementing<CNConstellationModule>();
                thisModule.radioFrequency = newFrequency;
                success = true;
            }
            else
            {
                List<ProtoPartSnapshot> parts = this.Vessel.protoVessel.protoPartSnapshots;

                for (int i = 0; i < parts.Count; i++)
                {
                    ProtoPartModuleSnapshot thisModule = parts.ElementAt(i).FindModule("CNConstellationModule");

                    if (thisModule == null)
                        continue;

                    success = thisModule.moduleValues.SetValue("radioFrequency", newFrequency);
                    break;
                }
            }

            if (success)
            {
                this.radioFrequency = newFrequency;
                CNCLog.Verbose("Update CommNet vessel '{0}''s frequency to {1}", this.Vessel.GetName(), newFrequency);
            }
            else
            {
                CNCLog.Error("Unable to update CommNet vessel '{0}''s frequency to {1}!", this.Vessel.GetName(), newFrequency);
            }
        }

        public short getRadioFrequency(bool forceRetrievalFromModule = false)
        {
            if (forceRetrievalFromModule)
            {
                if (this.Vessel.loaded)
                {
                    CNConstellationModule thisModule = this.Vessel.FindPartModuleImplementing<CNConstellationModule>();
                    this.radioFrequency = thisModule.radioFrequency;
                }
                else
                {
                    bool success = false;
                    List<ProtoPartSnapshot> parts = this.Vessel.protoVessel.protoPartSnapshots;

                    for (int i = 0; i < parts.Count && !success; i++)
                    {
                        ProtoPartModuleSnapshot thisModule = parts.ElementAt(i).FindModule("CNConstellationModule");

                        if (thisModule != null)
                        {
                            this.radioFrequency = short.Parse(thisModule.moduleValues.GetValue("radioFrequency"));
                            success = true;
                        }
                    }


                    if (!success) // fallback
                    {
                        CNCLog.Error("CommNet vessel '{0}' does not have the frequency module-value! Reset to freq {1}", this.Vessel.GetName(), this.radioFrequency);
                        this.radioFrequency = CNCSettings.Instance.PublicRadioFrequency;
                    }
                }
            }

            return this.radioFrequency;
        }
    }
}
