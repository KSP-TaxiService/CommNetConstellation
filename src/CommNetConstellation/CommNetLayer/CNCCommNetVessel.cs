using CommNet;
using CommNetConstellation.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// PartModule to be inserted into every ModuleCommand
    /// </summary>
    //This class is coupled with the MM patch (cnc_module_MM.cfg) that inserts CNConstellationModule into every command part
    public class CNConstellationModule : PartModule
    {
        [KSPField(isPersistant = true)] public short radioFrequency = CNCSettings.Instance.PublicRadioFrequency;
        [KSPField(isPersistant = true)] public bool communicationMembershipFlag = false;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "CommNet Constellation", active = true)]
        public void KSPEventConstellationSetup()
        {
            new VesselSetupDialog("Vessel - <color=#00ff00>Setup</color>", this.vessel, this.part, null).launch();
        }
    }

    /// <summary>
    /// Data structure for a CommNetVessel
    /// </summary>
    public class CNCCommNetVessel : CommNetManagerAPI.ModularCommNetVesselComponent
    {
        protected short radioFrequency;
        protected bool communicationMembershipFlag;

        /// <summary>
        /// Retrieve the CNConstellationModule data from the vessel
        /// </summary>
        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            validateAndUpgrade(this.Vessel);

            try
            {
                this.radioFrequency = getRadioFrequency(true);
                this.communicationMembershipFlag = getMembershipFlag(true);
            }
            catch (Exception e)
            {
                CNCLog.Error("Vessel '{0}' doesn't have any CommNet capability, likely a mislabelled junk", this.Vessel.GetName());
            }
        }

        /// <summary>
        /// Update the frequency of every command part of this vessel, even if one or more have different frequencies
        /// </summary>
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
                List<CNConstellationModule> modules = this.Vessel.FindPartModulesImplementing<CNConstellationModule>();
                for (int i = 0; i < modules.Count; i++)
                    modules[i].radioFrequency = newFrequency;
                success = true;
            }
            else
            {
                List<ProtoPartSnapshot> parts = this.Vessel.protoVessel.protoPartSnapshots;
                for (int i = 0; i < parts.Count; i++)
                {
                    ProtoPartModuleSnapshot thisModule = parts[i].FindModule("CNConstellationModule");
                    if (thisModule == null)
                        continue;

                    success = thisModule.moduleValues.SetValue("radioFrequency", newFrequency);
                }
            }

            if (success)
            {
                this.radioFrequency = newFrequency;
                CNCLog.Debug("Update CommNet vessel '{0}''s frequency to {1}", this.Vessel.GetName(), newFrequency);
            }
            else
            {
                CNCLog.Error("Can't update CommNet vessel '{0}''s frequency to {1}!", this.Vessel.GetName(), newFrequency);
            }
        }

        /// <summary>
        /// Update the frequency of the specific command part of this active vessel only
        /// </summary>
        public void updateRadioFrequency(short newFrequency, Part commandPart)
        {
            if (commandPart == null)
                return;

            CNConstellationModule cncModule = commandPart.FindModuleImplementing<CNConstellationModule>();
            CNCLog.Debug("Update the part '{1}''s freq in CommNet vessel '{0}' from {3} to {2}", this.Vessel.GetName(), commandPart.partInfo.title, newFrequency, cncModule.radioFrequency);
            cncModule.radioFrequency = newFrequency;
            getRadioFrequency(true);
        }

        /// <summary>
        /// If multiple command parts of this vessel have different frequencies, pick the frequency of the first part in order of part addition in editor
        /// </summary>
        public short getRadioFrequency(bool forceRetrievalFromModule = false)
        {
            if (forceRetrievalFromModule)
            {
                if (this.Vessel.loaded)
                    this.radioFrequency = firstCommandPartSelection(this.Vessel.parts).radioFrequency;
                else
                    this.radioFrequency = short.Parse(firstCommandPartSelection(this.Vessel.protoVessel.protoPartSnapshots).moduleValues.GetValue("radioFrequency"));

                CNCLog.Debug("Read the freq {1} from CommNet vessel '{0}'", this.Vessel.GetName(), this.radioFrequency);
            }

            return this.radioFrequency;
        }

        /// <summary>
        /// Update the membership flag of every command part of this vessel, even if one or more have different flags
        /// </summary>
        public void updateMembershipFlag(bool updatedFlag)
        {
            bool success = false;
            if (this.Vessel.loaded)
            {
                List<CNConstellationModule> modules = this.Vessel.FindPartModulesImplementing<CNConstellationModule>();
                for (int i = 0; i < modules.Count; i++)
                    modules[i].communicationMembershipFlag = updatedFlag;
                success = true;
            }
            else
            {
                List<ProtoPartSnapshot> parts = this.Vessel.protoVessel.protoPartSnapshots;
                for (int i = 0; i < parts.Count; i++)
                {
                    ProtoPartModuleSnapshot thisModule = parts[i].FindModule("CNConstellationModule");
                    if (thisModule == null)
                        continue;

                    success = thisModule.moduleValues.SetValue("communicationMembershipFlag", updatedFlag);
                }
            }

            if (success)
            {
                this.communicationMembershipFlag = updatedFlag;
                CNCLog.Debug("Update CommNet vessel '{0}''s membership flag to {1}", this.Vessel.GetName(), updatedFlag);
            }
            else
            {
                CNCLog.Error("Can't update CommNet vessel '{0}''s membership flag to {1}!", this.Vessel.GetName(), updatedFlag);
            }
        }

        /// <summary>
        /// Update the membership flag of the specific command part of this active vessel only
        /// </summary>
        public void updateMembershipFlag(bool updatedFlag, Part commandPart)
        {
            if (commandPart == null)
                return;

            CNConstellationModule cncModule = commandPart.FindModuleImplementing<CNConstellationModule>();
            CNCLog.Debug("Update the part '{1}''s membership flag in CommNet vessel '{0}' from {3} to {2}", this.Vessel.GetName(), commandPart.partInfo.title, updatedFlag, cncModule.communicationMembershipFlag);
            cncModule.communicationMembershipFlag = updatedFlag;
            getMembershipFlag(true);
        }

        /// <summary>
        /// If multiple command parts of this vessel have different flags, pick the membership flag of the first part in order of part addition in editor
        /// </summary>
        public bool getMembershipFlag(bool forceRetrievalFromModule = false)
        {
            if (forceRetrievalFromModule)
            {
                if (this.Vessel.loaded)
                    this.communicationMembershipFlag = firstCommandPartSelection(this.Vessel.parts).communicationMembershipFlag;
                else
                    this.communicationMembershipFlag = bool.Parse(firstCommandPartSelection(this.Vessel.protoVessel.protoPartSnapshots).moduleValues.GetValue("communicationMembershipFlag"));

                CNCLog.Debug("Read the membership flag '{1}' from CommNet vessel '{0}'", this.Vessel.GetName(), this.communicationMembershipFlag);
            }

            return this.communicationMembershipFlag;
        }

        /// <summary>
        /// Selection algorithm on multiple parts with different frequenices
        /// </summary>
        public CNConstellationModule firstCommandPartSelection(List<Part> parts)
        {
            for (int i = 0; i < parts.Count; i++) // grab the first command part (part list is sorted in order of part addition in editor)
            {
                CNConstellationModule thisModule;
                if ((thisModule = parts[i].FindModuleImplementing<CNConstellationModule>()) != null)
                {
                    return thisModule;
                }
            }

            return null;
        }

        /// <summary>
        /// Selection algorithm on multiple protoparts with different frequenices
        /// </summary>
        public ProtoPartModuleSnapshot firstCommandPartSelection(List<ProtoPartSnapshot> parts)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                ProtoPartModuleSnapshot partModule;
                if ((partModule = parts[i].FindModule("CNConstellationModule")) != null)
                {
                    return partModule;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if given vessel has CNConstellationModule and its attributes required, and if not, "upgrade" the vessel data
        /// </summary>
        public void validateAndUpgrade(Vessel thisVessel)
        {
            if (thisVessel == null)
                return;

            if (!thisVessel.loaded) // it seems KSP will automatically add/upgrade the active vessel (unconfirmed)
            {
                List<ProtoPartSnapshot> parts = thisVessel.protoVessel.protoPartSnapshots;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i].FindModule("ModuleCommand") != null) // check command parts only
                    {
                        ProtoPartModuleSnapshot cncModule;
                        if ((cncModule = parts[i].FindModule("CNConstellationModule")) == null) //check if CNConstellationModule is there
                        {
                            CNConstellationModule realcncModule = this.CommNetVessel.gameObject.AddComponent<CNConstellationModule>(); // don't use new keyword. PartModule is Monobehavior
                            parts[i].modules.Add(new ProtoPartModuleSnapshot(realcncModule));

                            CNCLog.Debug("CNConstellationModule is added to CommNet Vessel '{0}'", thisVessel.GetName());
                        }
                        else //check if all attributes are there
                        {
                            if (!cncModule.moduleValues.HasValue("radioFrequency"))
                            {
                                cncModule.moduleValues.AddValue("radioFrequency", CNCSettings.Instance.PublicRadioFrequency);
                                CNCLog.Debug("CNConstellationModule of CommNet Vessel '{0}' gets new attribute {1} - {2}", thisVessel.GetName(), "radioFrequency", CNCSettings.Instance.PublicRadioFrequency);
                            }

                            if (!cncModule.moduleValues.HasValue("communicationMembershipFlag"))
                            {
                                cncModule.moduleValues.AddValue("communicationMembershipFlag", false);
                                CNCLog.Debug("CNConstellationModule of CommNet Vessel '{0}' gets new attribute {1} - {2}", thisVessel.GetName(), "communicationMembershipFlag", false);
                            }
                        }
                    }
                }
            }
        }
    }
}
