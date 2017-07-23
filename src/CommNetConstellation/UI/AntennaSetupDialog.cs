using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit the constellation membership of this vessel (Controller)
    /// </summary>
    public class AntennaSetupDialog : AbstractDialog
    {
        private CNConstellationAntennaModule antennaModule;
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private DialogGUITextInput frequencyInput;
        private DialogGUITextInput nameInput;

        private DialogGUIImage constellationColorImage;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public AntennaSetupDialog(string title, Vessel vessel, Part antennaPart) : base("cncAntennaWindow",
                                                                                    title, 
                                                                                    0.5f, //x
                                                                                    0.5f, //y
                                                                                    250, //width
                                                                                    230, //height
                                                                                    new DialogOptions[] { DialogOptions.HideCloseButton})
        {
            this.hostVessel = vessel;
            this.antennaModule = antennaPart.FindModuleImplementing<CNConstellationAntennaModule>();
            this.description = string.Format("You are configuring this antenna '{0}'.", antennaPart.partInfo.title);

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 40, 12);
            nameInput = new DialogGUITextInput(antennaModule.Name, false, CNCSettings.MaxLengthName, setNameInput, 145, 25);
            DialogGUIButton defaultButton = new DialogGUIButton("Reset", defaultNameClick, 40, 32, false);
            DialogGUIHorizontalLayout nameGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { nameLabel, nameInput, defaultButton });
            listComponments.Add(nameGroup);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            frequencyInput = new DialogGUITextInput(antennaModule.Frequency.ToString(), false, CNCSettings.MaxDigits, setConstellFreq, 45, 25);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultFreqClick, false);
            DialogGUIHorizontalLayout freqGRoup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, publicButton });
            listComponments.Add(freqGRoup);

            constellationColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, Color.white, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(getConstellationName, 200, 12);
            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(5, 25, 5, 5), TextAnchor.MiddleCenter, new DialogGUIBase[] { constellationColorImage, constNameLabel });
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, constellationGroup));

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateAction, false);
            DialogGUIButton cancelButton = new DialogGUIButton("Cancel", delegate { this.dismiss(); }, false);
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, cancelButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(actionGroup);

            return listComponments;
        }

        /// <summary>
        /// For the dialog to get the constellation name based on the frequency in the textfield
        /// </summary>
        private string getConstellationName()
        {
            Constellation thisConstellation;

            try
            {
                thisConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text));
            }
            catch(FormatException e)
            {
                thisConstellation = null;
            }

            if (thisConstellation != null)
            {
                constellationColorImage.uiItem.GetComponent<RawImage>().color = thisConstellation.color;
                return thisConstellation.name;
            }
            else
            {
                constellationColorImage.uiItem.GetComponent<RawImage>().color = Color.clear;
                return "This frequency is unrecognised.";
            }
        }

        /// <summary>
        /// Action to update the attributes of the antenna
        /// </summary>
        private void updateAction()
        {
            try
            {
                try
                {
                    bool changesCommitted= false;
                    short inputFreq = short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text);
                    string inputName = nameInput.uiItem.GetComponent<TMP_InputField>().text.Trim();

                    //Check name
                    if (inputName.Length <= 0)
                    {
                        throw new Exception("Name cannot be empty");
                    }

                    //Check frequency
                    if (inputFreq < 0)
                    {
                        throw new Exception("Frequency cannot be negative");
                    }
                    else if (!Constellation.NonLinqAny(CNCCommNetScenario.Instance.constellations, inputFreq))
                    {
                        throw new Exception("Please choose an existing constellation");
                    }
                    else if (!Constellation.isFrequencyValid(inputFreq))
                    {
                        throw new Exception("Frequency must be between 0 and " + short.MaxValue);
                    }

                    //ALL OK
                    if (this.antennaModule.Frequency != inputFreq) // different frequency
                    {
                        this.antennaModule.Frequency = inputFreq;
                        ScreenMessage msg = new ScreenMessage(string.Format("Frequency is updated to {0}", inputFreq), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                        ScreenMessages.PostScreenMessage(msg);
                        changesCommitted = true;
                    }

                    if (!this.antennaModule.Name.Equals(inputName)) // different name
                    {
                        this.antennaModule.Name = inputName;
                        ScreenMessage msg = new ScreenMessage(string.Format("Antenna is renamed to '{0}'", this.antennaModule.Name), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                        ScreenMessages.PostScreenMessage(msg);
                        changesCommitted = true;
                    }

                    CNCLog.Debug("Updated antenna: {0}, {1}", inputName, inputFreq);

                    if (changesCommitted)
                    {
                        if (this.hostVessel != null)
                        {
                            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
                            cncVessel.OnAntennaChange();
                        }

                        this.dismiss();
                    }
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
        }

        /// <summary>
        /// Action to reset the antenna's frequency to the public one
        /// </summary>
        private void defaultFreqClick()
        {
            frequencyInput.uiItem.GetComponent<TMP_InputField>().text = CNCSettings.Instance.PublicRadioFrequency.ToString();
        }

        /// <summary>
        /// For the dialog to call upon new antenna-name input
        /// </summary>
        private string setNameInput(string newNameInput)
        {
            //do nothing
            return newNameInput;
        }

        /// <summary>
        /// For the dialog to call upon new frequency input
        /// </summary>
        private string setConstellFreq(string newFreqStr)
        {
            //do nothing
            return newFreqStr;
        }

        /// <summary>
        /// Action to revert the antenna's name back to the part name
        /// </summary>
        private void defaultNameClick()
        {
            nameInput.uiItem.GetComponent<TMP_InputField>().text = this.antennaModule.part.partInfo.title;
        }
    }
}
