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

        private Texture2D constellationTexture;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private DialogGUIImage colorImage;

        private Callback<Constellation> creationCallback;
        private Callback<Constellation> updateCallback;

        public ConstellationEditDialog(string dialogTitle, 
                                        Constellation thisConstellation, 
                                        Callback<Constellation> creationCallback, 
                                        Callback<Constellation> updateCallback) : base(dialogTitle,
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
                this.constellationTexture = UIUtils.createAndColorize(colorTexture, new Color(1f, 1f, 1f), this.conColor);
            }
            else
            {
                this.constellationTexture = colorTexture;
            }
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 52, 12);
            DialogGUITextInput nameInput = new DialogGUITextInput(this.conName, false, CNCSettings.Instance.MaxNumChars, setConNameFun, 170, 24);
            DialogGUIHorizontalLayout nameGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameLabel, nameInput });
            listComponments.Add(nameGroup);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            DialogGUITextInput frequencyInput = new DialogGUITextInput(this.conFreq.ToString(), false, 5, setConFreq, 45, 24);
            colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.zero, Color.white, this.constellationTexture);
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

        private string getMessage()
        {
            return "Message: " + this.message;
        }

        private string setConFreq(string newFreqStr)
        {
            try
            {
                short newFreq = short.Parse(newFreqStr);

                if (newFreq < 0)
                {
                    message = "<color=red>This frequency cannot be negative!</color>";
                    return newFreqStr;
                }
                else if (CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == newFreq))
                {
                    message = "<color=red>This frequency is already in use!</color>";
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
                message = "<color=red>This frequency must be numeric only!</color>";
                return newFreqStr;
            }
            catch(OverflowException e)
            {
                message = string.Format("<color=red>This frequency must be equal to or less than {0}!</color>", short.MaxValue);
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
                message = "<color=red>This name cannot be empty!</color>";
                return "";
            }
        }

        private void actionClick()
        {
            //TODO: lock public constellation's freq

            //Check errors
            if (CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == this.conFreq) && this.existingConstellation == null)
            {
                message = "<color=red>This frequency is already in use!</color>";
                return;
            }
            else if(this.conName.Trim().Length < 1)
            {
                message = "<color=red>This name cannot be empty!</color>";
                return;
            }

            if (this.existingConstellation == null && creationCallback!= null)
            {
                Constellation newConstellation = new Constellation(this.conFreq, this.conName, this.conColor);
                creationCallback(newConstellation);
                message = "Created successfully";
            }
            else if(this.existingConstellation != null && updateCallback != null)
            {
                this.existingConstellation.name = this.conName;
                this.existingConstellation.frequency = this.conFreq;
                this.existingConstellation.color = this.conColor;
                updateCallback(this.existingConstellation);
                message = "Updated successfully";
            }
            else
            {
                message = "Sorry, something is broken";
            }
        }

        private void colorEditClick()
        {
            new ColorPickerDialog(this.conColor, userChooseColor).launch();
        }

        public void userChooseColor(Color newChosenColor)
        {
            this.conColor = newChosenColor;
            Texture2D.DestroyImmediate(this.constellationTexture, true);
            this.constellationTexture = UIUtils.createAndColorize(colorTexture, new Color(1f, 1f, 1f), this.conColor);
            colorImage.uiItem.GetComponent<RawImage>().texture = constellationTexture;   
        }
    }
}
