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
        private Part antennaPart = null;
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private DialogGUITextInput frequencyInput;

        private DialogGUIImage constellationColorImage;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public AntennaSetupDialog(string title, Vessel vessel, Part antennaPart) : base(title, 
                                                                                    0.5f, //x
                                                                                    0.5f, //y
                                                                                    250, //width
                                                                                    190, //height
                                                                                    new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.antennaPart = antennaPart;
            this.description = string.Format("You are editing this antenna '{0}'.", this.antennaPart.partInfo.title);

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            CNConstellationAntennaModule antennaModule = antennaPart.FindModuleImplementing<CNConstellationAntennaModule>();

            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            frequencyInput = new DialogGUITextInput(antennaModule.Frequency.ToString(), false, CNCSettings.MaxDigits, setConstellFreq, 45, 25);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultClick, false);
            DialogGUIHorizontalLayout freqGRoup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, publicButton });
            listComponments.Add(freqGRoup);

            constellationColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, Color.white, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(getConstellationName, 200, 12);
            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { constellationColorImage, constNameLabel });
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
            Constellation thisConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text));

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

                CNConstellationAntennaModule antennaModule = antennaPart.FindModuleImplementing<CNConstellationAntennaModule>();
                antennaModule.Frequency = inputFreq;

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
        private void defaultClick()
        {
            frequencyInput.uiItem.GetComponent<TMP_InputField>().text = CNCSettings.Instance.PublicRadioFrequency.ToString();
            updateAction();            
        }
    }
}
