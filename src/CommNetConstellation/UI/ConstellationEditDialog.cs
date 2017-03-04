using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;
using CommNetConstellation.CommNetLayer;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit or create this constellation (Controller)
    /// </summary>
    public class ConstellationEditDialog : AbstractDialog
    {
        private string description = "You are creating a new constellation.";
        private string actionButtonText = "Create";

        private string constellName = "";
        private short constellFreq = 0;
        private Color conColor = Color.white;
        private Constellation existingConstellation = null;

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private DialogGUIImage constellationColorImage;

        private Callback<Constellation> creationCallback;
        private Callback<Constellation, short> updateCallback;

        public ConstellationEditDialog(string dialogTitle, 
                                        Constellation thisConstellation, 
                                        Callback<Constellation> creationCallback, 
                                        Callback<Constellation, short> updateCallback) : base(dialogTitle,
                                                                                    0.5f, //x
                                                                                    0.5f, //y
                                                                                    250, //width
                                                                                    200, //height
                                                                                    new DialogOptions[] {})
        {
            this.creationCallback = creationCallback;
            this.updateCallback = updateCallback;
            this.existingConstellation = thisConstellation;

            if(this.existingConstellation != null)
            {
                this.constellName = this.existingConstellation.name;
                this.constellFreq = this.existingConstellation.frequency;
                this.conColor = this.existingConstellation.color;

                this.description = string.Format("You are editing Constellation '{0}'.", this.constellName);
                this.actionButtonText = "Update";
            }
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 52, 12);
            DialogGUITextInput nameInput = new DialogGUITextInput(this.constellName, false, CNCSettings.MaxLengthName, setConstellName, 170, 25);
            DialogGUIHorizontalLayout nameGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameLabel, nameInput });
            listComponments.Add(nameGroup);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            DialogGUITextInput frequencyInput = new DialogGUITextInput(this.constellFreq.ToString(), false, CNCSettings.MaxDigits, setConstellFreq, 47, 25);
            constellationColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.zero, this.conColor, colorTexture);
            DialogGUIButton colorButton = new DialogGUIButton("Color", colorEditClick, null, 50, 24, false);
            DialogGUIHorizontalLayout freqColorGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(16), colorButton, constellationColorImage });
            listComponments.Add(freqColorGroup);

            DialogGUIButton updateButton = new DialogGUIButton(this.actionButtonText, actionClick, false);
            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup2);

            return listComponments;
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setConstellFreq(string newFreqStr)
        {
            try
            {
                try // input checks
                {
                    short newFreq = short.Parse(newFreqStr);

                    if (this.existingConstellation != null)
                    {
                        if (this.existingConstellation.frequency == CNCSettings.Instance.PublicRadioFrequency) //public one
                        {
                            if (newFreq != CNCSettings.Instance.PublicRadioFrequency)
                                throw new Exception("Public frequency "+ CNCSettings.Instance.PublicRadioFrequency + " is locked");
                        }
                    }
                    if (newFreq < 0)
                    {
                        throw new Exception("Frequency cannot be negative");
                    }
                    else if (CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == newFreq))
                    {
                        throw new Exception("Frequency is in use already");
                    }
                    else
                    {
                        this.constellFreq = newFreq;
                        return this.constellFreq.ToString();
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
            catch(Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }

            return newFreqStr;
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setConstellName(string newName)
        {
            if (newName.Trim().Length > 0)
            {
                this.constellName = newName;
                return this.constellName;
            }
            else
            {
                ScreenMessage msg = new ScreenMessage("<color=red>Name cannot be empty</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
                return newName;
            }
        }

        /// <summary>
        /// Action to create or update the constellation
        /// </summary>
        private void actionClick()
        {
            try
            {
                //Check errors
                if (CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == this.constellFreq) && this.existingConstellation == null)
                {
                    throw new Exception("Frequency is in use already");
                }
                else if (this.constellName.Trim().Length < 1)
                {
                    throw new Exception("Name cannot be empty");
                }
                else if (!Constellation.isFrequencyValid(this.constellFreq))
                {
                    throw new Exception("Frequency must be between 0 and " + short.MaxValue);
                }

                if (this.existingConstellation == null && creationCallback != null)
                {
                    Constellation newConstellation = new Constellation(this.constellFreq, this.constellName, this.conColor);
                    CNCCommNetScenario.Instance.constellations.Add(newConstellation);
                    creationCallback(newConstellation);

                    string message = string.Format("New constellation '{0}' of frequency {1} is created", this.constellName, this.constellFreq);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));
                }
                else if (this.existingConstellation != null && updateCallback != null)
                {
                    short prevFreq = this.existingConstellation.frequency;
                    this.existingConstellation.name = this.constellName;
                    this.existingConstellation.color = this.conColor;

                    if (this.existingConstellation.frequency != CNCSettings.Instance.PublicRadioFrequency) // this is not the public one
                        this.existingConstellation.frequency = this.constellFreq;

                    List<CNCCommNetVessel> affectedVessels = CNCCommNetScenario.Instance.getCommNetVessels().FindAll(x => x.getRadioFrequency() == prevFreq);
                    for (int i = 0; i < affectedVessels.Count; i++)
                        affectedVessels[i].updateRadioFrequency(this.existingConstellation.frequency);

                    updateCallback(this.existingConstellation, prevFreq);

                    string message = string.Format("Constellation '{0}' of frequency {1} is updated", this.constellName,this.constellFreq);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));
                }
                else
                {
                    throw new Exception("Something is broken :-(");
                }
            }
            catch (Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }
        }

        /// <summary>
        /// Launch the color picker to change the color
        /// </summary>
        private void colorEditClick()
        {
            new ColorPickerDialog(this.conColor, userChooseColor).launch();
        }

        /// <summary>
        /// Callback for the color picker to pass the new color to
        /// </summary>
        public void userChooseColor(Color newChosenColor)
        {
            this.conColor = newChosenColor;
            constellationColorImage.uiItem.GetComponent<RawImage>().color = this.conColor;
        }
    }
}
