using CommNet;
using CommNetConstellation.UI;
using Smooth.Algebraics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// PartModule to be inserted into every part having ModuleCommand module (probe cores and manned cockpits)
    /// </summary>
    //This class is coupled with the MM patch (cnc_module_MM.cfg) that inserts CNConstellationModule into every command part
    public class CNConstellationModule : PartModule
    {
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiActiveUnfocused = true, guiName = "CNC: Communication", active = true)]
        public void KSPEventVesselSetup()
        {             
            new VesselSetupDialog("Vessel - <color=#00ff00>Communication</color>", this.vessel, null).launch();
        }
    }

    /// <summary>
    /// PartModule to be inserted into every part having ModuleDataTransmitter module (antennas, probe cores and manned cockpits)
    /// </summary>
    //This class is coupled with the MM patch (cnc_module_MM.cfg) that inserts CNConstellationAntennaModule into every part
    public class CNConstellationAntennaModule : PartModule
    {
        [KSPField(isPersistant = true)] public short Frequency = CNCSettings.Instance.PublicRadioFrequency;
        [KSPField(isPersistant = true)] protected string OptionalName = "";
        [KSPField(isPersistant = true)] public bool InUse = true;

        public String Name
        {
            get { return (this.OptionalName.Length == 0) ? this.part.partInfo.title : this.OptionalName; }
            set { this.OptionalName = value; }
        }

        //TODO: auto-detect if antenna is deployed or retracted

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "CNC: Antenna Setup", active = true)]
        public void KSPEventAntennaConfig()
        {
            new AntennaSetupDialog("Antenna - <color=#00ff00>Setup</color>", this.vessel, this.part).launch();
        }
    }

    /// <summary>
    /// Independent-implementation data structure for an antenna part
    /// </summary>
    public class CNCAntennaPartInfo
    {
        public short frequency;
        public string name;
        public double antennaPower;
        public double antennaCombinableExponent;
        public bool antennaCombinable;
        public AntennaType antennaType;
        public Part partReference;
        public ProtoPartSnapshot partSnapshotReference = null;
        public bool inUse; // selected by user to be used
        public bool canComm; //fixed and deployable antennas
    }

    /// <summary>
    /// Data structure for a CommNetVessel
    /// </summary>
    public class CNCCommNetVessel : CommNetVessel, IPersistenceSave, IPersistenceLoad
    {
        public enum FrequencyListOperation
        {
            AutoBuild,
            LockList,
            //UpdateOnly //cannot find any use
        };

        //http://forum.kerbalspaceprogram.com/index.php?/topic/141574-kspfield-questions/&do=findComment&comment=2625815
        // cannot be serialized
        protected Dictionary<short, double> FrequencyDict = new Dictionary<short, double>();
        [Persistent] private List<short> FreqDictionaryKeys = new List<short>();
        [Persistent] private List<double> FreqDictionaryValues = new List<double>();
        [Persistent] public FrequencyListOperation FreqListOperation = FrequencyListOperation.AutoBuild; // initial value

        protected short strongestFreq = -1;
        protected List<CNCAntennaPartInfo> vesselAntennas = new List<CNCAntennaPartInfo>();

        /// <summary>
        /// Retrieve the CNC data from the vessel
        /// </summary>
        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            
            try
            {
                validateAndUpgrade(this.Vessel);
                OnAntennaChange();
            }
            catch (Exception e)
            {
                CNCLog.Error("Vessel '{0}' doesn't have any CommNet capability, likely a mislabelled junk or a kerbin on EVA", this.Vessel.GetName());
            }
        }

        /// <summary>
        /// Independent-implementation information on all antennas
        /// </summary>
        public List<CNCAntennaPartInfo> getAllAntennaInfo(bool readAntennaData = false)
        {
            if (readAntennaData || this.vesselAntennas == null)
                this.vesselAntennas = this.readAntennaData();

            return this.vesselAntennas;
        }

        /// <summary>
        /// Read the part data of an unloaded/loaded vessel and store in data structures
        /// </summary>
        protected List<CNCAntennaPartInfo> readAntennaData()
        {
            List<CNCAntennaPartInfo> antennas = new List<CNCAntennaPartInfo>();
            int numParts = (!this.vessel.loaded) ? this.vessel.protoVessel.protoPartSnapshots.Count : this.vessel.Parts.Count;

            //inspect each part
            for (int partIndex = 0; partIndex < numParts; partIndex++)
            {
                Part thisPart;
                ProtoPartSnapshot partSnapshot = null;

                if (this.Vessel.loaded)
                {
                    thisPart = this.vessel.Parts[partIndex];
                }
                else
                {
                    partSnapshot = this.vessel.protoVessel.protoPartSnapshots[partIndex];
                    thisPart = partSnapshot.partInfo.partPrefab;
                }

                bool populatedAntennaInfo = false;
                CNCAntennaPartInfo newAntennaPartInfo = new CNCAntennaPartInfo(); ;
                ProtoPartModuleSnapshot partModuleSnapshot = null;

                //inspect each module of the part
                for (int moduleIndex = 0; moduleIndex < thisPart.Modules.Count; moduleIndex++)
                {
                    PartModule thisPartModule = thisPart.Modules[moduleIndex];

                    if (thisPartModule is CNConstellationAntennaModule) // is it CNConstellationAntennaModule?
                    {
                        if (!this.Vessel.loaded)
                        {
                            partModuleSnapshot = partSnapshot.FindModule(thisPartModule, moduleIndex);

                            newAntennaPartInfo.frequency = short.Parse(partModuleSnapshot.moduleValues.GetValue("Frequency"));
                            string oname = partModuleSnapshot.moduleValues.GetValue("OptionalName");
                            newAntennaPartInfo.name = (oname.Length == 0) ? partSnapshot.partInfo.title : oname;
                            newAntennaPartInfo.inUse = bool.Parse(partModuleSnapshot.moduleValues.GetValue("InUse"));
                        }
                        else
                        {
                            CNConstellationAntennaModule antennaMod = (CNConstellationAntennaModule)thisPartModule;
                            newAntennaPartInfo.frequency = antennaMod.Frequency;
                            newAntennaPartInfo.name = antennaMod.Name;
                            newAntennaPartInfo.inUse = antennaMod.InUse;
                        }

                        populatedAntennaInfo = true;
                    }
                    else if (thisPartModule is ICommAntenna) // is it ModuleDataTransmitter?
                    {
                        ICommAntenna thisAntenna = thisPartModule as ICommAntenna;

                        if (!this.Vessel.loaded)
                            partModuleSnapshot = partSnapshot.FindModule(thisPartModule, moduleIndex);

                        newAntennaPartInfo.antennaPower = (!this.vessel.loaded) ? thisAntenna.CommPowerUnloaded(partModuleSnapshot) : thisAntenna.CommPower;
                        newAntennaPartInfo.antennaCombinable = thisAntenna.CommCombinable;
                        newAntennaPartInfo.antennaCombinableExponent = thisAntenna.CommCombinableExponent;
                        newAntennaPartInfo.antennaType = thisAntenna.CommType;
                        newAntennaPartInfo.partReference = thisPart; //unique ID for part is not available
                        newAntennaPartInfo.partSnapshotReference = partSnapshot;
                        newAntennaPartInfo.canComm = (!this.vessel.loaded) ? thisAntenna.CanCommUnloaded(partModuleSnapshot) : thisAntenna.CanComm();

                        populatedAntennaInfo = true;
                    }
                }

                if(populatedAntennaInfo) // valid info?
                    antennas.Add(newAntennaPartInfo);
            }

            return antennas;
        }

        /// <summary>
        /// Build the vessel's frequency list from chosen antennas
        /// </summary>
        protected Dictionary<short, double> buildFrequencyList(List<CNCAntennaPartInfo> antennas)
        {
            Dictionary<short, double> freqDict = new Dictionary<short, double>();
            Dictionary<short, double[]> powerDict = new Dictionary<short, double[]>();

            const int COMINDEX = 0;
            const int MAXINDEX = 1;

            //read each antenna
            for(int i=0; i<antennas.Count; i++)
            {
                if (!antennas[i].inUse || !antennas[i].canComm) // deselected or retracted
                    continue;

                if(!powerDict.ContainsKey(antennas[i].frequency))//not found
                    powerDict.Add(antennas[i].frequency, new double[] { 0.0, 0.0 });

                if (antennas[i].antennaCombinable) // TODO: revise to best antenna power * (total power / best power) * avg(all expo)
                    powerDict[antennas[i].frequency][COMINDEX] += (powerDict[antennas[i].frequency][COMINDEX]==0.0) ? antennas[i].antennaPower : antennas[i].antennaCombinableExponent * antennas[i].antennaPower;
                else
                    powerDict[antennas[i].frequency][MAXINDEX] = Math.Max(powerDict[antennas[i].frequency][MAXINDEX], antennas[i].antennaPower);
            }

            //consolidate into vessel's list of frequencies and their com powers
            foreach (short freq in powerDict.Keys)
            {
                freqDict.Add(freq, powerDict[freq].Max());
            }

            return freqDict;
        }

        /// <summary>
        /// Get the list of frequencies only
        /// </summary>
        public List<short> getFrequencies()
        {
            return this.FrequencyDict.Keys.ToList();
        }

        /// <summary>
        /// Get the max com power of a given frequency
        /// </summary>
        public double getMaxComPower(short frequency)
        {
            if (this.FrequencyDict.ContainsKey(frequency))
                return this.FrequencyDict[frequency];
            else
                return 0.0;
        }

        /// <summary>
        /// Get the frequency of the largest Com Power
        /// </summary>
        public short getStrongestFrequency()
        {
            if (this.strongestFreq < 0)
            {
                this.FrequencyDict = buildFrequencyList(this.vesselAntennas);
                this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);
            }

            return this.strongestFreq;
        }

        /// <summary>
        /// Find the frequency with the largest Com Power
        /// </summary>
        private short computeStrongestFrequency(Dictionary<short, double> dict)
        {
            if (dict.Count < 1)
                return -1;
            else if (dict.Count == 1)
                return dict.Keys.First();

            List<KeyValuePair<short, double>> decreasingFreqs= dict.OrderByDescending(x => x.Value).ToList();
            short freq = decreasingFreqs[0].Key;

            if(freq == CNCSettings.Instance.PublicRadioFrequency)
            {
                if (decreasingFreqs[0].Value == decreasingFreqs[1].Value)
                    freq = decreasingFreqs[1].Key; // pick next freq of same comm power
            }
            
            return freq;
        }

        /// <summary>
        /// Check if the vessel's frequency list can be edited
        /// </summary>
        public bool isFreqListEditable()
        {
            if (this.FreqListOperation != CNCCommNetVessel.FrequencyListOperation.LockList)
                return true;

            ScreenMessage msg = new ScreenMessage("Note: Lock List mode is in effect.", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
            ScreenMessages.PostScreenMessage(msg);
            return false;
        }

        /// <summary>
        /// Notify CommNet vessel on antenna change (like changing frequency and deploy/retract antenna)
        /// </summary>
        public void OnAntennaChange()
        {
            this.vesselAntennas = readAntennaData();

            switch (this.FreqListOperation)
            {
                case FrequencyListOperation.AutoBuild:
                    rebuildFreqList();
                    break;
                case FrequencyListOperation.LockList: // dont change current freq dict
                    this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);
                    break;
            }
        }

        /// <summary>
        /// Rebuild the frequency list from all antennas
        /// </summary>
        public void rebuildFreqList(bool readAntennaData = false)
        {
            if (!isFreqListEditable())
            {
                this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);
                return;
            }

            if(readAntennaData)
                this.vesselAntennas = this.readAntennaData();

            this.FrequencyDict = buildFrequencyList(this.vesselAntennas);
            this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);
        }

        /// <summary>
        /// Add a new (or existing) frequency and its comm power to vessel's frequency list
        /// </summary>
        public void addToFreqList(short frequency, double commPower)
        {
            if (!isFreqListEditable()) return;

            if (this.FrequencyDict.ContainsKey(frequency))
                this.FrequencyDict[frequency] = commPower;
            else
                this.FrequencyDict.Add(frequency, commPower);
        }

        /// <summary>
        /// Drop the specific frequency from the vessel's frequency list
        /// </summary>
        public void removeFromFreqList(short frequency)
        {
            if (!isFreqListEditable()) return; 

            this.FrequencyDict.Remove(frequency);
        }

        /// <summary>
        /// Clear the vessel's frequency list
        /// </summary>
        public void clearFreqList()
        {
            if (!isFreqListEditable()) return;

            this.FrequencyDict.Clear();
        }

        /// <summary>
        /// Replace one frequency in the particular antenna
        /// </summary>
        public void updateFrequency(CNCAntennaPartInfo partInfo, short newFrequency)
        {
            if (!Constellation.isFrequencyValid(newFrequency))
            {
                CNCLog.Error("New frequency {0} is out of the range [0,{1}]!", newFrequency, short.MaxValue);
                return;
            }

            partInfo.frequency = newFrequency;

            if (this.Vessel.loaded)
            {
                partInfo.partReference.FindModuleImplementing<CNConstellationAntennaModule>().Frequency = newFrequency;
            }
            else
            {
                partInfo.partSnapshotReference.FindModule("CNConstellationAntennaModule").moduleValues.SetValue("Frequency", newFrequency);
            }

            CNCLog.Debug("Update the antenna of CommNet vessel '{0}' to {1}", this.Vessel.GetName(), newFrequency);
        }

        /// <summary>
        /// Replace one frequency in all antennas
        /// </summary>
        public void replaceAllFrequencies(short oldFrequency, short newFrequency)
        {
            if (!Constellation.isFrequencyValid(newFrequency))
            {
                CNCLog.Error("New frequency {0} is out of the range [0,{1}]!", newFrequency, short.MaxValue);
                return;
            }

            if (this.Vessel.loaded)
            {
                List<CNConstellationAntennaModule> mods = this.Vessel.FindPartModulesImplementing<CNConstellationAntennaModule>().FindAll(x => x.Frequency == oldFrequency);
                for (int i = 0; i < mods.Count; i++)
                    mods[i].Frequency = newFrequency;
            }
            else
            {
                for (int i = 0; i < this.vessel.protoVessel.protoPartSnapshots.Count; i++)
                {
                    ProtoPartModuleSnapshot cncAntMod = this.vessel.protoVessel.protoPartSnapshots[i].FindModule("CNConstellationAntennaModule");
                    if (short.Parse(cncAntMod.moduleValues.GetValue("Frequency")) == oldFrequency)
                        cncAntMod.moduleValues.SetValue("Frequency", newFrequency);
                }
            }

            getAllAntennaInfo(true);
            CNCLog.Debug("Update all occurrences of frequency {1} in CommNet vessel '{0}' to {2}", this.Vessel.GetName(), oldFrequency, newFrequency);
        }

        /// <summary>
        /// Turn on or off the specific antenna
        /// </summary>
        public void toggleAntenna(CNCAntennaPartInfo partInfo, bool inUse)
        {
            partInfo.inUse = inUse;

            if (this.Vessel.loaded)
            {
                partInfo.partReference.FindModuleImplementing<CNConstellationAntennaModule>().InUse = inUse;
            }
            else
            {
                partInfo.partSnapshotReference.FindModule("CNConstellationAntennaModule").moduleValues.SetValue("InUse", inUse);
            }

            CNCLog.Debug("Set the antenna '{0}' of CommNet vessel '{1}' to {2}", partInfo.name, this.Vessel.GetName(), inUse);
        }

        /// <summary>
        /// Check if given vessel has CNConstellationModule and its attributes required, and if not, "upgrade" the vessel data
        /// </summary>
        public void validateAndUpgrade(Vessel thisVessel)
        {
            if (thisVessel == null)
                return;
            if (thisVessel.loaded) // it seems KSP will automatically add/upgrade the active vessel (unconfirmed)
                return;

            CNCLog.Debug("Unloaded CommNet vessel '{0}' is validated and upgraded", thisVessel.GetName());

            if (thisVessel.protoVessel != null)
            {
                List<ProtoPartSnapshot> parts = thisVessel.protoVessel.protoPartSnapshots;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i].FindModule("ModuleCommand") != null) // check command parts only
                    {
                        ProtoPartModuleSnapshot cncModule;
                        if ((cncModule = parts[i].FindModule("CNConstellationModule")) == null) //check if CNConstellationModule is there
                        {
                            CNConstellationModule realcncModule = gameObject.AddComponent<CNConstellationModule>(); // don't use new keyword. PartModule is Monobehavior
                            parts[i].modules.Add(new ProtoPartModuleSnapshot(realcncModule));

                            CNCLog.Verbose("CNConstellationModule is added to CommNet Vessel '{0}'", thisVessel.GetName());
                        }
                        else //check if all attributes are or should not be there
                        {
                            if (cncModule.moduleValues.HasValue("radioFrequency")) //obsolete
                                cncModule.moduleValues.RemoveValue("radioFrequency");

                            if (cncModule.moduleValues.HasValue("communicationMembershipFlag")) //obsolete
                                cncModule.moduleValues.RemoveValue("communicationMembershipFlag");
                        }
                    }

                    if (parts[i].FindModule("ModuleDataTransmitter") != null) // check antennas, probe cores and manned cockpits
                    {
                        ProtoPartModuleSnapshot cncModule;
                        if ((cncModule = parts[i].FindModule("CNConstellationAntennaModule")) == null) //check if CNConstellationAntennaModule is there
                        {
                            CNConstellationAntennaModule realcncModule = gameObject.AddComponent<CNConstellationAntennaModule>(); // don't use new keyword. PartModule is Monobehavior
                            parts[i].modules.Add(new ProtoPartModuleSnapshot(realcncModule));

                            CNCLog.Verbose("CNConstellationAntennaModule is added to CommNet Vessel '{0}'", thisVessel.GetName());
                        }
                    }
                } // end of part loop
            }
        }

        protected override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);

            if (gameNode.HasNode(GetType().FullName))
                gameNode.RemoveNode(GetType().FullName);

            gameNode.AddNode(ConfigNode.CreateConfigFromObject(this));
        }

        protected override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            if(gameNode.HasNode(GetType().FullName))
                ConfigNode.LoadObjectFromConfig(this, gameNode.GetNode(GetType().FullName));
        }

        public void PersistenceSave()
        {
            FreqDictionaryKeys = FrequencyDict.Keys.ToList();
            FreqDictionaryValues = FrequencyDict.Values.ToList();
        }

        public void PersistenceLoad()
        {
            FrequencyDict = Enumerable.Range(0, FreqDictionaryKeys.Count).ToDictionary(idx => FreqDictionaryKeys[idx], idx => FreqDictionaryValues[idx]);
        }
    }
}
