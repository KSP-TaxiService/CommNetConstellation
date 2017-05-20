using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit the constellation membership of this vessel (Controller)
    /// </summary>
    public class VesselSetupDialog : AbstractDialog
    {
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private Callback<Vessel, short> updateCallback;

        private DialogGUIVerticalLayout frequencyRowLayout;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel vessel, Callback<Vessel, short>  updateCallback) : base(title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                250, //width
                                                                                                                240, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.updateCallback = updateCallback;
            this.description = string.Format("The frequency list of this vessel '{0}' is used to communicate with other vessels of the same frequencies. "+
                                             "A number of tools is available to manage this list, included antenna activation/deactivation.", this.hostVessel.vesselName);

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();
            List<short> vesselFrequencyList = ((CNCCommNetVessel) this.hostVessel.Connection).getFrequencies();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description + "\n\n", false, false) }));

            //frequency list
            listComponments.Add(new DialogGUILabel("\n<b>Current frequency list</b>", false, false));
            DialogGUIBase[] frequencyRows = new DialogGUIBase[vesselFrequencyList.Count + 1];
            frequencyRows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < vesselFrequencyList.Count; i++)
            {
                frequencyRows[i + 1] = createFrequencyRow(vesselFrequencyList[i]);
            }

            frequencyRowLayout = new DialogGUIVerticalLayout(10, 100, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, frequencyRows);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, frequencyRowLayout));

            //selectable antennas
            listComponments.Add(new DialogGUILabel("\n<b>Antennas</b>", false, false));

            DialogGUIVerticalLayout vesselLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { });
            vesselLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUIVerticalLayout antennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout comPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout drainPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            for (int i = 0; i < vesselAntennas.Count; i++)
            {
                Tuple<ModuleDataTransmitter, bool> thisAntenna = vesselAntennas[i];
                ModuleDataTransmitter antennaModule = thisAntenna.Item1;
                int antennaIndex = i; // antennaModules.Count doesn't work due to the compiler optimization

                DialogGUIToggle toggleBtn = new DialogGUIToggle(thisAntenna.Item2, antennaModule.part.partInfo.title, delegate (bool b) { vesselAntennaSelected(b, antennaIndex); }, 170, 32);
                DialogGUILabel comPowerLabel = new DialogGUILabel(string.Format("Com power: {0:0.00}", UiUtils.RoundToNearestMetricFactor(antennaModule.CommPower)), style); comPowerLabel.size = new Vector2(150, 32);
                DialogGUILabel powerDrainLabel = new DialogGUILabel(string.Format("Drain: {0:0.00} charge/s", antennaModule.DataResourceCost), style); powerDrainLabel.size = new Vector2(150, 32);

                antennaColumn.AddChild(toggleBtn);
                comPowerColumn.AddChild(comPowerLabel);
                drainPowerColumn.AddChild(powerDrainLabel);
            }

            vesselLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { antennaColumn, comPowerColumn, drainPowerColumn }));
            DialogGUIScrollList antennaScrollPane = new DialogGUIScrollList(new Vector2(AntennaSimulator.dialogWidth - 50, AntennaSimulator.dialogHeight / 3), false, true, vesselLayout);
            components.Add(antennaScrollPane);


            //tools
            listComponments.Add(new DialogGUILabel("\n<b>Management tools</b>", false, false));


            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, constellationGroup));

            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, constellationGroup));


            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { constellationColorImage, constNameLabel });

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            frequencyInput = new DialogGUITextInput(inputFreq.ToString(), false, CNCSettings.MaxDigits, setConstellFreq, 45, 25);
            DialogGUIHorizontalLayout freqGRoup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(100) });
            listComponments.Add(freqGRoup);

            constellationColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, Color.white, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(getConstellationName, 200, 12);
            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { constNameLabel, constellationColorImage });
            listComponments.Add(constellationGroup);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultClick, false);
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { updateButton, publicButton });
            listComponments.Add(actionGroup);

            return listComponments;
        }

        private DialogGUIHorizontalLayout createFrequencyRow(short freq)
        {
            Color color = Constellation.getColor(freq);
            string name = Constellation.getName(freq);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, color, colorTexture);
            DialogGUILabel nameLabel = new DialogGUILabel(name, 150, 12);
            DialogGUILabel eachFreqLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(color), freq), 20, 12);
            DialogGUILabel freqPowerLabel = new DialogGUILabel(string.Format("Combined Comm Power: {0} TODO", UIUtils.RoundToNearestMetricFactor(1234)), 80, 12);
            return new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { colorImage, nameLabel, eachFreqLabel, freqPowerLabel });
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setConstellFreq(string newFreqStr)
        {
            try
            {
                try // input validations
                {
                    short newFreq = short.Parse(newFreqStr);

                    if (newFreq < 0)
                        throw new Exception("Frequency cannot be negative");
                }
                catch (FormatException e)
                {
                    throw new FormatException("Frequency must be numeric only");
                }
                catch (OverflowException e)
                {
                    throw new OverflowException(string.Format("Frequency must be equal to or less than {0}", short.MaxValue));
                }
            }
            catch (Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }

            return newFreqStr;
        }

        /// <summary>
        /// For the dialog to get the constellation name based on the frequency in the textfield
        /// </summary>
        private string getConstellationName()
        {
            try
            {
                Constellation thisConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text));

                if (thisConstellation != null)
                {
                    constellationColorImage.uiItem.GetComponent<RawImage>().color = thisConstellation.color;
                    return "<b>Constellation:</b> " + thisConstellation.name;
                }
            } catch (Exception e) { }

            constellationColorImage.uiItem.GetComponent<RawImage>().color = Color.clear;
            return "<b>Constellation:</b> Unrecognised";
        }

        /// <summary>
        /// Action to update the frequency of the vessel
        /// </summary>
        private void updateClick()
        {
            try
            {
                short inputFreq = short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text);

                //Check errors
                if (!CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == inputFreq))
                {
                    throw new Exception("Please choose an existing constellation");
                }
                else if (!Constellation.isFrequencyValid(inputFreq))
                {
                    throw new Exception("Frequency must be between 0 and "+short.MaxValue);
                }

                if (rightClickedPart != null) // either in editor or flight
                {
                    if (this.hostVessel != null) // flight
                    {
                        CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                        //cv.updateRadioFrequency(inputFreq, rightClickedPart);
                    }
                    else // editor
                    {
                        CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                        //cncModule.radioFrequency = inputFreq;
                    }

                    string message = string.Format("Frequency of {0} is updated to {1}", rightClickedPart.partInfo.title, inputFreq);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));
                }
                else if (this.hostVessel != null) // tracking station
                {
                    CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                    //short prevFrequency = cv.getRadioFrequency();
                    //cv.updateRadioFrequency(inputFreq);

                    string message = string.Format("Individual frequencies of {0} are updated to {1}", this.hostVessel.GetName(), inputFreq);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));

                    //updateCallback(this.hostVessel, prevFrequency);
                }
                else
                {
                    throw new Exception("Something is broken ;_;");
                }
            }
            catch (Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }
        }

        /// <summary>
        /// Action to reset the vessel's frequency to the public one
        /// </summary>
        private void defaultClick()
        {
            frequencyInput.uiItem.GetComponent<TMP_InputField>().text = CNCSettings.Instance.PublicRadioFrequency.ToString();
            updateClick();            
        }
    }
}
