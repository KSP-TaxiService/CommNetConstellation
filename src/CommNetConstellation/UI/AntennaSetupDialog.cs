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
    public class AntennaSetupDialog : AbstractDialog
    {
        private CNConstellationAntennaModule antennaModule;
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private DialogGUITextInput frequencyInput;
        private DialogGUITextInput nameInput;

        private DialogGUIImage constellationColorImage;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public AntennaSetupDialog(string title, Vessel vessel, Part antennaPart) : base(title, 
                                                                                    0.5f, //x
                                                                                    0.5f, //y
                                                                                    250, //width
                                                                                    230, //height
                                                                                    new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.antennaModule = antennaPart.FindModuleImplementing<CNConstellationAntennaModule>();
            this.description = string.Format("You are editing this antenna '{0}'.", antennaPart.partInfo.title);

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

            return listComponments;
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

                    //All ok
                    updateAction();
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
        /// Action to set the frequency of the antenna
        /// </summary>
        private void updateAction()
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

                this.antennaModule.Frequency = inputFreq;

                string message = string.Format("Antenna frequency is updated to {0}", inputFreq);
                ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));
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
            updateAction();            
        }

        /// <summary>
        /// For the dialog to call upon new antenna-name input
        /// </summary>
        private string setNameInput(string newNameInput)
        {
            if (!this.antennaModule.Name.Equals(newNameInput.Trim())) // different name
            {
                this.antennaModule.Name = newNameInput.Trim();
                ScreenMessage msg = new ScreenMessage(string.Format("This antenna is renamed to '{0}'.", this.antennaModule.Name), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }

            return newNameInput;
        }

        /// <summary>
        /// Action to revert the antenna's name back to the part name
        /// </summary>
        private void defaultNameClick()
        {
            nameInput.uiItem.GetComponent<TMP_InputField>().text = this.antennaModule.part.partInfo.title;
            this.antennaModule.Name = ""; // blank

            string message = string.Format("This ground station's name is reverted to '{0}'.", this.antennaModule.Name);
            ScreenMessage msg = new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
            ScreenMessages.PostScreenMessage(msg);
        }
    }
}
