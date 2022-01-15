using CommNet;
using CommNetConstellation.UI;
using System;
using System.Collections.Generic;
using Expansions.Serenity.DeployedScience.Runtime;
using KSP.Localization;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// PartModule to be inserted into every part having ModuleCommand module (probe cores and manned cockpits)
    /// </summary>
    //This class is coupled with the MM patch (cnc_module_MM.cfg) that inserts CNConstellationModule into every command part
    public class CNConstellationModule : PartModule
    {
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiActiveUnfocused = true, guiName = "#CNC_GUIName_Communication", active = true)]//CNC: Communication
        public void KSPEventVesselSetup()
        {
            new VesselSetupDialog(Localizer.Format("#CNC_GUIName_VesselSetupDialog_title"), this.vessel, null).launch();//"Vessel - <color=#00ff00>Communication</color>"
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

        [KSPField(isPersistant = true)] public double CosAngle = -1.0; //by default, disabled until third-party mod enables it via MM
        [KSPField] public Guid AntennaTarget = Guid.Empty; //TODO: implement targeting

        public String Name
        {
            get { return (this.OptionalName.Length == 0) ? this.part.partInfo.title : this.OptionalName; }
            set { this.OptionalName = value; }
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "#CNC_GUIName_AntennaSetup", active = true)]//CNC: Antenna Setup
        public void KSPEventAntennaConfig()
        {
            new AntennaSetupDialog(Localizer.Format("#CNC_GUIName_AntennaSetupDialog_title"), this.vessel, this.part).launch();//"Antenna - <color=#00ff00>Setup</color>"
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
        public Guid Target;
        public double CosAngle;
    }

    /// <summary>
    /// Data structure for a CommNetVessel
    /// </summary>
    public class CNCCommNetVessel : CommNetManagerAPI.ModularCommNetVesselComponent, IPersistenceSave, IPersistenceLoad
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

        //for low-gc operations
        protected short[] sorted_frequency_array;

        protected short strongestFreq = -1;
        protected List<CNCAntennaPartInfo> vesselAntennas = new List<CNCAntennaPartInfo>();
        protected bool stageActivated = false;
        public bool IsCommandable = true;

        public List<CNCAntennaPartInfo> Antennas { get { return vesselAntennas; } }

        /// <summary>
        /// Retrieve the CNC data from the vessel, in addition of stock network joining
        /// </summary>
        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            
            try
            {
                validateAndUpgrade(this.Vessel);

                if(this.Vessel.vesselType == VesselType.DeployedScienceController)
                {
                    this.IsCommandable = true;
                    this.vesselAntennas = this.readAntennaData();
                    this.FrequencyDict = buildFrequencyList(this.vesselAntennas);
                    this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);
                    GameEvents.onGroundSciencePartEnabledStateChanged.Add(this.groundSciencePartStateChange);
                    return;
                }
                else if (readCommandData().Count <= 0) //no probe core/command module to "process" signal
                {
                    this.IsCommandable = false;
                    throw new Exception();
                }
                else
                {
                    this.IsCommandable = true;
                }

                if (this.FreqListOperation == CNCCommNetVessel.FrequencyListOperation.AutoBuild)
                {
                    this.vesselAntennas = this.readAntennaData();
                    this.FrequencyDict = buildFrequencyList(this.vesselAntennas);
                }
                
                this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);

                GameEvents.onStageActivate.Add(stageActivate);
                GameEvents.onVesselWasModified.Add(vesselModified);
                addListenerToAntennaDeployment(this.vesselAntennas);
            }
            catch (Exception e)
            {
                CNCLog.Verbose("Vessel '{0}' doesn't have any CommNet capability due to no probe core/command part or a kerbin on EVA", this.Vessel.GetName());
            }
        }

        protected void OnDestroy()
        {
            if (HighLogic.CurrentGame == null)
                return;
            //if (HighLogic.LoadedScene != GameScenes.FLIGHT)
            //    return;

            GameEvents.onStageActivate.Remove(stageActivate);
            GameEvents.onVesselWasModified.Remove(vesselModified);
            removeListenerToAntennaDeployment(this.vesselAntennas);

            if (this.Vessel.vesselType == VesselType.DeployedScienceController)
            {
                GameEvents.onGroundSciencePartEnabledStateChanged.Remove(this.groundSciencePartStateChange);
            }
        }

        /// <summary>
        /// GameEvent of staging a vessel
        /// </summary>
        private void stageActivate(int stageIndex)
        {
            if (this.Vessel.isActiveVessel)
            {
                this.stageActivated = true;
            }
        }

        /// <summary>
        /// GameEvent of vessel being modified
        /// </summary>
        private void vesselModified(Vessel thisVessel)
        {
            if (this.Vessel.isActiveVessel && this.stageActivated) // decouple event
            {
                CNCLog.Verbose("Active CommNet Vessel '{0}' is staged. Rebuilding the freq list on suriving antennas...", this.Vessel.vesselName);

                if (readCommandData().Count <= 0) //no probe core/command module to "process" signal
                {
                    this.IsCommandable = false;
                }
                else
                {
                    this.IsCommandable = true;

                    //force-rebuild freq list to stop players from abusing LockList
                    this.vesselAntennas = this.readAntennaData();
                    this.FrequencyDict = buildFrequencyList(this.vesselAntennas);
                    this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);
                }

                this.stageActivated = false;
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
                CNCAntennaPartInfo newAntennaPartInfo = new CNCAntennaPartInfo();
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

                            if (partModuleSnapshot == null)
                            {
                                //initialise and add to proto part
                                ProtoPartModuleSnapshot newCNCAntMod = new ProtoPartModuleSnapshot(thisPartModule);
                                newCNCAntMod.moduleValues.SetValue("Frequency", CNCSettings.Instance.PublicRadioFrequency);
                                newCNCAntMod.moduleValues.SetValue("OptionalName", "");
                                newCNCAntMod.moduleValues.SetValue("InUse", true);
                                newCNCAntMod.moduleValues.SetValue("CosAngle", 0.0);
                                partSnapshot.modules.Add(newCNCAntMod);

                                partModuleSnapshot = partSnapshot.FindModule(thisPartModule, moduleIndex);
                                if(partModuleSnapshot == null) //something goes wrong
                                {
                                    CNCLog.Error("Unable to add CNConstellationAntennaModule to proto part snapshot of '{0}'!", this.vessel.vesselName);
                                    break;
                                }
                            }

                            string oname = partModuleSnapshot.moduleValues.GetValue("OptionalName");
                            newAntennaPartInfo.name = (oname.Length == 0) ? partSnapshot.partInfo.title : oname;
                            if (!short.TryParse(partModuleSnapshot.moduleValues.GetValue("Frequency"), out newAntennaPartInfo.frequency))
                            {
                                newAntennaPartInfo.frequency = CNCSettings.Instance.PublicRadioFrequency; // default value
                            }
                            if (!bool.TryParse(partModuleSnapshot.moduleValues.GetValue("InUse"), out newAntennaPartInfo.inUse))
                            {
                                newAntennaPartInfo.inUse = true; // default value
                            }
                            if (!double.TryParse(partModuleSnapshot.moduleValues.GetValue("CosAngle"), out newAntennaPartInfo.CosAngle))
                            {
                                newAntennaPartInfo.CosAngle = 0.0; // default value
                            }
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

                if (populatedAntennaInfo) // valid info?
                {
                    antennas.Add(newAntennaPartInfo);
                    CNCLog.Debug("CommNet Vessel '{0}' has antenna '{1}' of {2} and {3} power (w/o range modifier)", this.Vessel.GetName(), newAntennaPartInfo.name, newAntennaPartInfo.frequency, newAntennaPartInfo.antennaPower);
                }
            }

            return antennas;
        }

        /// <summary>
        /// Read the command part data of an unloaded/loaded vessel
        /// </summary>
        protected List<ModuleCommand> readCommandData()
        {
            if(this.vessel.loaded)//quick check
            {
                return this.vessel.FindPartModulesImplementing<ModuleCommand>();
            }

            //manually read on unloaded vessel
            List<ModuleCommand> cmds = new List<ModuleCommand>();
            int numParts = this.vessel.protoVessel.protoPartSnapshots.Count;

            //inspect each part
            for (int partIndex = 0; partIndex < numParts; partIndex++)
            {
                ProtoPartSnapshot partSnapshot = this.vessel.protoVessel.protoPartSnapshots[partIndex];
                Part thisPart = partSnapshot.partInfo.partPrefab;
                cmds.AddRange(thisPart.Modules.GetModules<ModuleCommand>());
            }

            return cmds;
        }

        /// <summary>
        /// Trap the antenna deployment
        /// </summary>
        protected void addListenerToAntennaDeployment(List<CNCAntennaPartInfo> antennas)
        {
            if (antennas == null)
                return;

            for (int i=0; i< antennas.Count; i++)
            {
                Part thisPart = antennas[i].partReference;
                ModuleDeployableAntenna deployMod = thisPart.FindModuleImplementing<ModuleDeployableAntenna>();
                if(deployMod != null)
                {
                    deployMod.OnStop.Add(OnAntennaDeployment);
                }
            }
        }

        /// <summary>
        /// Callback when antenna is extended or retracted
        /// </summary>
        private void OnAntennaDeployment(float data)
        {
            if (!this.Vessel.isActiveVessel)
                return;

            CNCLog.Verbose("Antenna in active CommNet Vessel '{0}' is extended/retracted. Rebuilding the freq list ...", this.Vessel.vesselName);
            OnAntennaChange();
        }

        /// <summary>
        /// Unhook the antenna deployment
        /// </summary>
        protected void removeListenerToAntennaDeployment(List<CNCAntennaPartInfo> antennas)
        {
            if (antennas == null)
                return;

            for (int i = 0; i < antennas.Count; i++)
            {
                Part thisPart = antennas[i].partReference;
                ModuleDeployableAntenna deployMod = thisPart.FindModuleImplementing<ModuleDeployableAntenna>();
                if (deployMod != null)
                {
                    deployMod.OnStop.Remove(OnAntennaDeployment);
                }
            }
        }

        /// <summary>
        /// Build the vessel's frequency list from chosen antennas
        /// </summary>
        protected Dictionary<short, double> buildFrequencyList(List<CNCAntennaPartInfo> antennas)
        {
            Dictionary<short, List<double>> combinepowerFreqDict = new Dictionary<short, List<double>>();
            Dictionary<short, List<double>> expoFreqDict = new Dictionary<short, List<double>>();
            Dictionary<short, double> noncombinepowerDict = new Dictionary<short, double>();
            List<short> allFreqs = new List<short>();

            //read each antenna
            for (int i=0; i<antennas.Count; i++)
            {
                if (!antennas[i].inUse || !antennas[i].canComm) // deselected or retracted
                    continue;

                short thisFreq = antennas[i].frequency;

                if (!allFreqs.Contains(thisFreq))//not found
                {
                    allFreqs.Add(thisFreq);
                    combinepowerFreqDict.Add(thisFreq, new List<double> { });
                    expoFreqDict.Add(thisFreq, new List<double> { });
                    noncombinepowerDict.Add(thisFreq, 0.0);
                }

                if (antennas[i].antennaCombinable)
                {
                    expoFreqDict[thisFreq].Add(antennas[i].antennaPower*antennas[i].antennaCombinableExponent*(double)HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().rangeModifier);
                    combinepowerFreqDict[thisFreq].Add(antennas[i].antennaPower*(double)HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().rangeModifier);
                }
                else
                {
                    noncombinepowerDict[thisFreq] = Math.Max(noncombinepowerDict[thisFreq], antennas[i].antennaPower*(double)HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().rangeModifier);
                }
            }

            //consolidate into vessel's list of frequencies and their com powers
            Dictionary<short, double> freqDict = new Dictionary<short, double>();
            for(int i=0; i<allFreqs.Count;i++) // each freq
            {
                short freq = allFreqs[i];

                //best antenna power * (total power / best power) ^ (sum(expo*power)/total power)
                double weightedExpo = GameUtils.NonLinqSum(expoFreqDict[freq]) / GameUtils.NonLinqSum(combinepowerFreqDict[freq]);
                double combinedPower = (combinepowerFreqDict[freq].Count == 0)? 0.0 : GameUtils.NonLinqMax(combinepowerFreqDict[freq]) * Math.Pow((GameUtils.NonLinqSum(combinepowerFreqDict[freq]) / GameUtils.NonLinqMax(combinepowerFreqDict[freq])), weightedExpo);
                freqDict.Add(freq, Math.Max(combinedPower, noncombinepowerDict[freq]));
                CNCLog.Debug("CommNet Vessel '{0}''s freq list has freq {1} of {2} power", this.Vessel.GetName(), freq, Math.Max(combinedPower, noncombinepowerDict[freq]));
            }

            regenerateFrequencyArray(freqDict);

            return freqDict;
        }

        /// <summary>
        /// Get the *sorted* array of frequencies only
        /// </summary>
        public short[] getFrequencyArray()
        {
            if(sorted_frequency_array == null)
            {
                regenerateFrequencyArray(this.FrequencyDict);
            }
            return sorted_frequency_array;
        }

        /// <summary>
        /// Get the *unsorted* list of frequencies only
        /// </summary>
        public List<short> getFrequencyList()
        {
            List<short> freqs = new List<short>();
            var itr = this.FrequencyDict.Keys.GetEnumerator();
            while (itr.MoveNext())
            {
                freqs.Add(itr.Current);
            }

            return freqs;
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
                this.vesselAntennas = readAntennaData();
                this.FrequencyDict = buildFrequencyList(this.vesselAntennas);
                this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);
            }

            return this.strongestFreq;
        }

        /// <summary>
        /// Perform the updates on CommNet vessel's communication
        /// </summary>
        protected override void UpdateComm()
        {
            base.UpdateComm();

            //Preventative measure on null antenna range curve due to 3rd-party mods
            //Effect: cause commNode.antennaRelay.rangeCurve.Evaluate(normalizedRange) to fail *without* throwing exception
            if (this.CommNetVessel.Comm.antennaRelay.rangeCurve == null || this.CommNetVessel.Comm.antennaTransmit.rangeCurve == null)
            {
                if (this.CommNetVessel.Comm.antennaRelay.rangeCurve == null && this.CommNetVessel.Comm.antennaTransmit.rangeCurve != null)
                {
                    this.CommNetVessel.Comm.antennaRelay.rangeCurve = this.CommNetVessel.Comm.antennaTransmit.rangeCurve;
                }
                else if (this.CommNetVessel.Comm.antennaRelay.rangeCurve != null && this.CommNetVessel.Comm.antennaTransmit.rangeCurve == null)
                {
                    this.CommNetVessel.Comm.antennaTransmit.rangeCurve = this.CommNetVessel.Comm.antennaRelay.rangeCurve;
                }
                else //failsafe
                {
                    CNCLog.Error("CommNetVessel '{0}' has no range curve for both relay and transmit AntennaInfo! Fall back to a simple curve.", this.Vessel.GetName());
                    this.CommNetVessel.Comm.antennaTransmit.rangeCurve = this.CommNetVessel.Comm.antennaRelay.rangeCurve = new DoubleCurve(new DoubleKeyframe[] { new DoubleKeyframe(0.0, 0.0), new DoubleKeyframe(1.0, 1.0) });
                }
            }

            this.CommNetVessel.Comm.antennaTransmit.power = getMaxComPower(this.strongestFreq);
            //commented as range multiplier is applied by the mod
            //this.comm.antennaTransmit.power *= (double)HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().rangeModifier;
            //this.comm.antennaRelay.power *= (double)HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().rangeModifier;
        }

        /// <summary>
        /// Find the frequency with the largest Com Power
        /// </summary>
        protected short computeStrongestFrequency(Dictionary<short, double> dict)
        {
            if (dict.Count < 1)
            {
                //CNCLog.Verbose("CommNet Vessel '{0}' does not have any freq", this.Vessel.GetName()); //print this in every frame; disabled
                return -1;
            }
            else if (dict.Count == 1)
            {
                var itr = dict.Keys.GetEnumerator(); itr.MoveNext();
                short onefreq = itr.Current;
                CNCLog.Verbose("CommNet Vessel '{0}' has the single freq {1} of {2} power", this.Vessel.GetName(), onefreq, dict[onefreq]);
                return onefreq;
            }

            //List<KeyValuePair<short, double>> decreasingFreqs= dict.OrderByDescending(x => x.Value).ToList();
            List<KeyValuePair<short, double>> decreasingFreqs = new List<KeyValuePair<short, double>>();
            var itr2 = dict.GetEnumerator();
            while(itr2.MoveNext())
            {
                decreasingFreqs.Add(itr2.Current);
            }
            decreasingFreqs.Sort((a, b) => b.Value.CompareTo(a.Value));

            short freq = decreasingFreqs[0].Key;

            if(freq == CNCSettings.Instance.PublicRadioFrequency)
            {
                if (decreasingFreqs[0].Value == decreasingFreqs[1].Value)
                    freq = decreasingFreqs[1].Key; // pick next freq of same comm power
            }

            CNCLog.Verbose("CommNet Vessel '{0}' has the strongest freq {1} of {2} power", this.Vessel.GetName(), freq, dict[freq]);
            return freq;
        }

        /// <summary>
        /// Check if the vessel's frequency list can be edited
        /// </summary>
        public bool isFreqListEditable()
        {
            if (this.FreqListOperation != CNCCommNetVessel.FrequencyListOperation.LockList)
            {
                return true;
            }

            ScreenMessage msg = new ScreenMessage(Localizer.Format("#CNC_ScreenMsg_isFreqListEditable", this.vessel.GetDisplayName()), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER);//string.Format("Frequency list of Vessel '{0}' is locked.", )
            ScreenMessages.PostScreenMessage(msg);
            CNCLog.Verbose("CommNet Vessel '{0}''s freq list is locked", this.Vessel.GetName());
            return false;
        }

        /// <summary>
        /// Notify CommNet vessel on antenna change (like changing frequency and deploy/retract antenna)
        /// </summary>
        public void OnAntennaChange()
        {
            this.vesselAntennas = readAntennaData();
            isFreqListEditable();

            switch (this.FreqListOperation)
            {
                case FrequencyListOperation.AutoBuild:
                    rebuildFreqList(true);
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

            regenerateFrequencyArray(this.FrequencyDict);

            CNCLog.Debug("CommNet Vessel '{0}''s freq list gains {1} ({2} power)", this.Vessel.GetName(), frequency, commPower);
        }

        /// <summary>
        /// Drop the specific frequency from the vessel's frequency list
        /// </summary>
        public void removeFromFreqList(short frequency)
        {
            if (!isFreqListEditable()) return; 

            this.FrequencyDict.Remove(frequency);
            regenerateFrequencyArray(this.FrequencyDict);

            CNCLog.Debug("CommNet Vessel '{0}''s freq list loses freq {1}", this.Vessel.GetName(), frequency);
        }

        /// <summary>
        /// Clear the vessel's frequency list
        /// </summary>
        public void clearFreqList()
        {
            if (!isFreqListEditable()) return;

            this.FrequencyDict.Clear();
            regenerateFrequencyArray(this.FrequencyDict);

            CNCLog.Debug("CommNet Vessel '{0}''s freq list is cleared", this.Vessel.GetName());
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
                var cncAntMod = partInfo.partSnapshotReference.FindModule("CNConstellationAntennaModule");
                if(cncAntMod != null) cncAntMod.moduleValues.SetValue("Frequency", newFrequency);
            }

            CNCLog.Verbose("Update the antenna of CommNet vessel '{0}' to {1}", this.Vessel.GetName(), newFrequency);
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
                    short freq;
                    ProtoPartModuleSnapshot cncAntMod = this.vessel.protoVessel.protoPartSnapshots[i].FindModule("CNConstellationAntennaModule");
                    if (cncAntMod != null && short.TryParse(cncAntMod.moduleValues.GetValue("Frequency"), out freq) && freq == oldFrequency)
                        cncAntMod.moduleValues.SetValue("Frequency", newFrequency);
                }
            }

            getAllAntennaInfo(true);
            CNCLog.Verbose("Update all occurrences of frequency {1} in CommNet vessel '{0}' to {2}", this.Vessel.GetName(), oldFrequency, newFrequency);
        }

        /// <summary>
        /// Turn on or off the specific antenna
        /// </summary>
        public void toggleAntenna(CNCAntennaPartInfo partInfo, bool inUse)
        {
            if(!partInfo.canComm) // antenna is not deployed
            {
                ScreenMessage msg = new ScreenMessage(Localizer.Format("#CNC_ScreenMsg_toggleAntenna", partInfo.name), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER);//string.Format("Antenna '{0}' is not deployed.", )
                ScreenMessages.PostScreenMessage(msg);
                CNCLog.Verbose("Cannot set the non-deployed antenna '{0}' of CommNet vessel '{1}' to {2}", partInfo.name, this.Vessel.GetName(), inUse);
                return;
            }

            partInfo.inUse = inUse;

            if (this.Vessel.loaded)
            {
                partInfo.partReference.FindModuleImplementing<CNConstellationAntennaModule>().InUse = inUse;
            }
            else
            {
                var cncAntMod = partInfo.partSnapshotReference.FindModule("CNConstellationAntennaModule");
                if(cncAntMod != null) cncAntMod.moduleValues.SetValue("InUse", inUse);
            }

            CNCLog.Verbose("Set the antenna '{0}' of CommNet vessel '{1}' to {2}", partInfo.name, this.Vessel.GetName(), inUse);
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
                            CNConstellationModule realcncModule = this.CommNetVessel.gameObject.AddComponent<CNConstellationModule>(); // don't use new keyword. PartModule is Monobehavior
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
                        else //check if all attributes are or should not be there
                        {
                            if (!cncModule.moduleValues.HasValue("CosAngle"))
                                cncModule.moduleValues.AddValue("CosAngle", -1.0);
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
            List<short> keys = new List<short>();
            var keyItr = FrequencyDict.Keys.GetEnumerator();
            while(keyItr.MoveNext())
            {
                keys.Add(keyItr.Current);
            }
            FreqDictionaryKeys = keys;

            List<double> values = new List<double>();
            var valueItr = FrequencyDict.Values.GetEnumerator();
            while (valueItr.MoveNext())
            {
                values.Add(valueItr.Current);
            }
            FreqDictionaryValues = values;
        }

        public void PersistenceLoad()
        {
            //FrequencyDict = Enumerable.Range(0, FreqDictionaryKeys.Count).ToDictionary(idx => FreqDictionaryKeys[idx], idx => FreqDictionaryValues[idx]);
            FrequencyDict = new Dictionary<short, double>();
            for (int i=0; i< FreqDictionaryKeys.Count; i++)
            {
                FrequencyDict.Add(FreqDictionaryKeys[i], FreqDictionaryValues[i]);
            }
            regenerateFrequencyArray(FrequencyDict);
        }

        /// <summary>
        /// Regenerate frequency array used for low-gc operations
        /// </summary>
        protected void regenerateFrequencyArray(Dictionary<short, double> dict)
        {
            if (dict.Keys.Count == 0)
            {
                sorted_frequency_array = new short[] { };
            }

            int i = 0;
            sorted_frequency_array = new short[dict.Keys.Count];

            var valueItr = dict.Keys.GetEnumerator();
            while (valueItr.MoveNext())
            {
                sorted_frequency_array[i++] = valueItr.Current;
            }

            GameUtils.Quicksort(sorted_frequency_array, 0, sorted_frequency_array.Length - 1);
        }

        /// <summary>
        /// GameEvent of any deployed science part changed in state
        /// </summary>
        protected void groundSciencePartStateChange(ModuleGroundSciencePart sciencePart)
        {
            if (sciencePart is ModuleGroundExpControl)
            {
                //toggle antenna module based on part enabled toggling
                Part controlStation = sciencePart.part;
                CNConstellationAntennaModule antennaMod = controlStation.FindModuleImplementing<CNConstellationAntennaModule>();
                if(antennaMod != null)
                {
                    antennaMod.InUse = sciencePart.Enabled;
                    //Comment: Control Station has CanComm=false at this point -> require player action
                }

                this.rebuildFreqList(true);
            }
            else if (sciencePart is ModuleGroundCommsPart)
            {
                this.rebuildFreqList(true);
            }
        }

        /// <summary>
        /// Check if this and target vessels can establish connection of specific frequency
        /// </summary>
        public bool isConnectionEligible(CNCCommNetVessel otherVessel, short targetFreq)
        {
            for(int i = 0; i < this.vesselAntennas.Count; i++)
            {
                if(this.vesselAntennas[i].frequency == targetFreq)
                {
                    for(int j = 0; j < otherVessel.Antennas.Count; j++)
                    {
                        if (otherVessel.Antennas[j].frequency == targetFreq)
                        {
                            if(this.vesselAntennas[i].antennaType == AntennaType.RELAY || otherVessel.Antennas[j].antennaType == AntennaType.RELAY)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false; //cannot connect to each other
        }
    }
}
