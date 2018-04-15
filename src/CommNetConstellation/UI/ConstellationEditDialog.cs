using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using CommNetConstellation.CommNetLayer;
using TMPro;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit or create this constellation (Controller)
    /// </summary>
    public class ConstellationEditDialog : AbstractDialog
    {
        private string description = "You are creating a new constellation.";
        private string actionButtonText = "Create";

        private Color constellColor = Color.white;
        private Constellation existingConstellation = null;

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private DialogGUIImage constellationColorImage;

        private Callback<Constellation> creationCallback;
        private Callback<Constellation, short> updateCallback;
        private DialogGUITextInput nameInput;
        private DialogGUITextInput frequencyInput;

        public ConstellationEditDialog(string dialogTitle, 
                                        Constellation thisConstellation, 
                                        Callback<Constellation> creationCallback, 
                                        Callback<Constellation, short> updateCallback) : base("constellationEdit",
                                                                                    dialogTitle,
                                                                                    0.5f, //x
                                                                                    0.5f, //y
                                                                                    250, //width
                                                                                    170, //height
                                                                                    new DialogOptions[] { DialogOptions.HideCloseButton})
        {
            this.creationCallback = creationCallback;
            this.updateCallback = updateCallback;
            this.existingConstellation = thisConstellation;

            if(this.existingConstellation != null)
            {
                this.constellColor = this.existingConstellation.color;

                this.description = string.Format("You are editing Constellation '{0}'.", this.existingConstellation.name);
                this.actionButtonText = "Update";
            }

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            string constellName = "";
            short constellFreq = 0;
            if (this.existingConstellation != null)
            {
                constellName = this.existingConstellation.name;
                constellFreq = this.existingConstellation.frequency;
            }

            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 60, 12);
            nameInput = new DialogGUITextInput(constellName, false, CNCSettings.MaxLengthName, setConstellName, 170, 25);
            DialogGUIHorizontalLayout nameGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameLabel, nameInput });
            listComponments.Add(nameGroup);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 60, 12);
            frequencyInput = new DialogGUITextInput(constellFreq.ToString(), false, CNCSettings.MaxDigits, setConstellFreq, 55, 25);
            constellationColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, this.constellColor, colorTexture);
            DialogGUIButton colorButton = new DialogGUIButton("Color", colorEditClick, null, 50, 25, false);
            DialogGUIHorizontalLayout freqColorGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(15), colorButton, constellationColorImage });
            listComponments.Add(freqColorGroup);

            DialogGUIButton updateButton = new DialogGUIButton(this.actionButtonText, actionClick, false);
            DialogGUIButton cancelButton = new DialogGUIButton("Cancel", delegate { this.dismiss(); }, false);
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, cancelButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(actionGroup);

            return listComponments;
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setConstellFreq(string newFreqStr)
        {
            //do nothing
            return newFreqStr;
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setConstellName(string newName)
        {
            //do nothing
            return newName;
        }

        /// <summary>
        /// Action to create or update the constellation
        /// </summary>
        private void actionClick()
        {
            try
            {
                try
                {
                    short constellFreq = short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text);
                    string constellName = nameInput.uiItem.GetComponent<TMP_InputField>().text;

                    //Check name
                    if (constellName.Length <= 0)
                    {
                        throw new Exception("Name cannot be empty");
                    }

                    //Check frequency
                    if (constellFreq < 0)
                    {
                        throw new Exception("Frequency cannot be negative");
                    }
                    else if (!Constellation.isFrequencyValid(constellFreq))
                    {
                        throw new Exception("Frequency must be between 0 and " + short.MaxValue);
                    }
                    else if (this.existingConstellation != null)
                    {
                        if (this.existingConstellation.frequency == CNCSettings.Instance.PublicRadioFrequency) //public one
                        {
                            if (constellFreq != CNCSettings.Instance.PublicRadioFrequency)
                                throw new Exception("Public frequency " + CNCSettings.Instance.PublicRadioFrequency + " is locked");
                        }
                        /*
                        else if(constellFreq == CNCSettings.Instance.PublicRadioFrequency) // not public but new freq is public
                        {
                            throw new Exception("New frequency cannot be " + CNCSettings.Instance.PublicRadioFrequency);
                        }
                        */
                        else if (Constellation.NonLinqAny(CNCCommNetScenario.Instance.constellations, constellFreq) && this.existingConstellation.frequency != constellFreq)
                        {
                            throw new Exception("Frequency is in use already");
                        }
                    }
                    else if (this.existingConstellation == null && Constellation.NonLinqAny(CNCCommNetScenario.Instance.constellations, constellFreq))
                    {
                        throw new Exception("Frequency is in use already");
                    }

                    //ALL OK
                    if (this.existingConstellation == null && creationCallback != null)
                    {
                        Constellation newConstellation = new Constellation(constellFreq, constellName, this.constellColor);
                        CNCCommNetScenario.Instance.constellations.Add(newConstellation);
                        creationCallback(newConstellation);

                        string message = string.Format("New constellation '{0}' of frequency {1} is created", constellName, constellFreq);
                        ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER));

                        CNCLog.Debug("New constellation: {0}, {1}", constellName, constellFreq);

                        this.dismiss();
                    }
                    else if (this.existingConstellation != null && updateCallback != null)
                    {
                        bool changesCommitted = false;
                        short prevFreq = this.existingConstellation.frequency;

                        if (this.existingConstellation.frequency != constellFreq) // new frequency
                        {    
                            this.existingConstellation.frequency = constellFreq;

                            List<CNCCommNetVessel> affectedVessels = CNCCommNetScenario.Instance.getCommNetVessels().FindAll(x => x.getFrequencies().Contains(prevFreq));
                            for (int i = 0; i < affectedVessels.Count; i++)
                            {
                                affectedVessels[i].replaceAllFrequencies(prevFreq, this.existingConstellation.frequency);
                                affectedVessels[i].OnAntennaChange();
                            }

                            List<CNCCommNetHome> affectedStations = CNCCommNetScenario.Instance.groundStations.FindAll(x => x.Frequencies.Contains(prevFreq));
                            for(int i=0; i < affectedStations.Count; i++)
                            {
                                affectedStations[i].replaceFrequency(prevFreq, this.existingConstellation.frequency);
                            }

                            ScreenMessage msg = new ScreenMessage(string.Format("Constellation has the new frequency {0}", constellFreq), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                            ScreenMessages.PostScreenMessage(msg);
                            changesCommitted = true;
                        }

                        if(!this.existingConstellation.name.Equals(constellName)) // different name
                        {
                            this.existingConstellation.name = constellName;
                            string message = string.Format("Constellation is renamed to '{0}'", constellName);
                            ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER));
                            changesCommitted = true;
                        }

                        if (!this.existingConstellation.color.Equals(this.constellColor)) // new color
                        {
                            this.existingConstellation.color = this.constellColor;
                            string message = string.Format("Constellation color becomes '{0}'", UIUtils.colorToHex(this.constellColor));
                            ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER));
                            changesCommitted = true;
                        }

                        CNCLog.Debug("Updated constellation: {0}, {1}", constellName, constellFreq);

                        if (changesCommitted)
                        {
                            updateCallback(this.existingConstellation, prevFreq);
                            this.dismiss();
                        }
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
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                ScreenMessages.PostScreenMessage(msg);
            }
        }

        /// <summary>
        /// Launch the color picker to change the color
        /// </summary>
        private void colorEditClick()
        {
            new ColorPickerDialog(this.constellColor, userChooseColor).launch();
        }

        /// <summary>
        /// Callback for the color picker to pass the new color to
        /// </summary>
        public void userChooseColor(Color newChosenColor)
        {
            this.constellColor = newChosenColor;
            constellationColorImage.uiItem.GetComponent<RawImage>().color = this.constellColor;
        }
    }
}
