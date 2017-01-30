using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;
using CommNetConstellation.CommNetLayer;

namespace CommNetConstellation.UI
{
    public class ConstellationEditDialog : AbstractDialog
    {
        private string description = "You are creating a new constellation.";
        private string actionButtonText = "Create";
        private string message = "Ready";

        private Constellation existingConstellation = null;
        private string conName = "";
        private short conFreq = 0;
        private Color conColor = Color.white;

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private DialogGUIImage colorImage;

        private Callback<Constellation> creationCallback;
        private Callback<Constellation, short> updateCallback;

        public ConstellationEditDialog(string dialogTitle, 
                                        Constellation thisConstellation, 
                                        Callback<Constellation> creationCallback, 
                                        Callback<Constellation, short> updateCallback) : base(dialogTitle,
                                                                                    0.5f, //x
                                                                                    0.5f, //y
                                                                                    250, //width
                                                                                    255, //height
                                                                                    new DialogOptions[] {})
        {
            this.creationCallback = creationCallback;
            this.updateCallback = updateCallback;
            this.existingConstellation = thisConstellation;

            if(this.existingConstellation != null)
            {
                this.conName = this.existingConstellation.name;
                this.conFreq = this.existingConstellation.frequency;
                this.conColor = this.existingConstellation.color;

                this.description = string.Format("You are editing Constellation '{0}'.", this.conName);
                this.actionButtonText = "Update";
            }
        }

        private string getMessage()
        {
            return "Message: " + this.message;
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 52, 12);
            DialogGUITextInput nameInput = new DialogGUITextInput(this.conName, false, CNCSettings.Instance.MaxNumChars, setConNameFun, 170, 25);
            DialogGUIHorizontalLayout nameGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameLabel, nameInput });
            listComponments.Add(nameGroup);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            DialogGUITextInput frequencyInput = new DialogGUITextInput(this.conFreq.ToString(), false, 5, setConFreq, 45, 25);
            colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.zero, this.conColor, colorTexture);
            DialogGUIButton colorButton = new DialogGUIButton("Color", colorEditClick, null, 50, 24, false);
            DialogGUIHorizontalLayout freqColorGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(18), colorButton, colorImage });
            listComponments.Add(freqColorGroup);

            DialogGUIButton updateButton = new DialogGUIButton(this.actionButtonText, actionClick, false);
            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup2);

            DialogGUILabel messageLabel = new DialogGUILabel(getMessage, true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(false, false, 4, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        private string setConFreq(string newFreqStr)
        {
            try
            {
                short newFreq = short.Parse(newFreqStr);

                if (this.existingConstellation != null)
                {
                    if (this.existingConstellation.frequency == CNCSettings.Instance.PublicRadioFrequency) //public one
                    {
                        if (newFreq != CNCSettings.Instance.PublicRadioFrequency)
                        {
                            message = "<color=red>Sorry, this public frequency is locked</color>";
                            return CNCSettings.Instance.PublicRadioFrequency.ToString();
                        }
                    }
                }
                if (newFreq < 0)
                {
                    message = "<color=red>This frequency cannot be negative</color>";
                    return newFreqStr;
                }
                else if (CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == newFreq))
                {
                    message = "<color=red>This frequency is already in use</color>";
                    return newFreqStr;
                }
                else
                {
                    this.conFreq = newFreq;
                    return this.conFreq.ToString();
                }
            }
            catch(FormatException e)
            {
                message = "<color=red>This frequency must be numeric only</color>";
                return newFreqStr;
            }
            catch(OverflowException e)
            {
                message = string.Format("<color=red>This frequency must be equal to or less than {0}</color>", short.MaxValue);
                return newFreqStr;
            }
        }

        private string setConNameFun(string newName)
        {
            if (newName.Trim().Length > 0)
            {
                this.conName = newName;
                return this.conName;
            }
            else
            {
                message = "<color=red>This name cannot be empty</color>";
                return "";
            }
        }

        private void actionClick()
        {
            //Check errors
            if (CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == this.conFreq) && this.existingConstellation == null)
            {
                message = "<color=red>This frequency is already in use</color>";
                return;
            }
            else if(this.conName.Trim().Length < 1)
            {
                message = "<color=red>This name cannot be empty</color>";
                return;
            }
            else if(!Constellation.isFrequencyValid(this.conFreq))
            {
                message = string.Format("<color=red>This frequency must be between 0 and {0}</color>", short.MaxValue);
                return;
            }

            if (this.existingConstellation == null && creationCallback!= null)
            {
                Constellation newConstellation = new Constellation(this.conFreq, this.conName, this.conColor);
                CNCCommNetScenario.Instance.constellations.Add(newConstellation);
                creationCallback(newConstellation);
                message = "Created successfully";
            }
            else if(this.existingConstellation != null && updateCallback != null)
            {
                short prevFreq = this.existingConstellation.frequency;
                this.existingConstellation.name = this.conName;
                this.existingConstellation.color = this.conColor;

                if(this.existingConstellation.frequency != CNCSettings.Instance.PublicRadioFrequency) // this is not the public one
                    this.existingConstellation.frequency = this.conFreq;

                List<CNCCommNetVessel> affectedVessels = CNCCommNetScenario.Instance.getCommNetVessels().FindAll(x => x.getRadioFrequency() == prevFreq);
                for (int i = 0; i < affectedVessels.Count; i++)
                    affectedVessels[i].updateRadioFrequency(this.existingConstellation.frequency);

                updateCallback(this.existingConstellation, prevFreq);
                message = "Updated successfully";
            }
            else
            {
                message = "Something is broken :(";
            }
        }

        private void colorEditClick()
        {
            new ColorPickerDialog(this.conColor, userChooseColor).launch();
        }

        public void userChooseColor(Color newChosenColor)
        {
            this.conColor = newChosenColor;
            colorImage.uiItem.GetComponent<RawImage>().color = this.conColor;
        }
    }
}
