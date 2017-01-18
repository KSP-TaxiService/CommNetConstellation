using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CommNetConstellation.UI
{
    public class ConstellationEditDialog : AbstractDialog
    {
        private string constellationNameTextfield = "";
        private int frequency = 0;
        private string description = "You are creating a new constellation.";
        private string actionButtonText = "Create";
        private Constellation constellation = null;
        private string message = "Ready";
        private Color constColor = Color.white;
        
        private Texture2D colorButtonIcon;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        private DialogGUITextInput nameInput;
        private DialogGUITextInput frequencyInput;
        private DialogGUIButton colorButton;

        public ConstellationEditDialog(string dialogTitle, Constellation thisConstellation) : base(dialogTitle,
                                                                                                        0.5f, //x
                                                                                                        0.5f, //y
                                                                                                        250, //width
                                                                                                        255, //height
                                                                                                        new DialogOptions[] {})
        {
            this.constellation = thisConstellation;
            if(this.constellation != null)
            {
                this.description = string.Format("You are editing Constellation '{0}'.", this.constellation.name);
                this.actionButtonText = "Update";
                this.constellationNameTextfield = thisConstellation.name;
                this.frequency = thisConstellation.frequency;
                this.constColor = thisConstellation.color;
                this.colorButtonIcon = UIUtils.createAndColorize(colorTexture, new Color(1f, 1f, 1f), this.constColor);
            }
            else
            {
                this.colorButtonIcon = colorTexture;
            }
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 40, 12);
            nameInput = new DialogGUITextInput(this.constellationNameTextfield, false, CNCSettings.Instance.MaxNumChars, null, 130, 32);
            DialogGUIHorizontalLayout nameGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameLabel, nameInput, new DialogGUIFlexibleSpace() });
            listComponments.Add(nameGroup);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Radio frequency</b>", 50, 24);
            frequencyInput = new DialogGUITextInput(this.frequency.ToString(), false, 5, null, 45, 32);
            DialogGUILabel colorLabel = new DialogGUILabel("<b>Color</b>", 32, 12);

            UIStyle btnStyle = UIUtils.createImageButtonStyle(colorButtonIcon);
            colorButton = new DialogGUIButton("", delegate { colorEditClick(this.constColor); }, null, 32, 32, false, btnStyle);
            colorButton.image = btnStyle.normal.background;

            DialogGUIHorizontalLayout freqColorGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(21), colorLabel, colorButton });
            listComponments.Add(freqColorGroup);

            DialogGUIButton updateButton = new DialogGUIButton(this.actionButtonText, actionClick, false);
            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup2);

            DialogGUILabel messageLabel = new DialogGUILabel("Message: "+message, true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(false, false, 4, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        private void actionClick()
        {

        }

        private void colorEditClick(Color chosenColor)
        {
            new ColorPickerDialog(chosenColor, userChooseColor).launch();
        }

        public void userChooseColor(Color newChosenColor)
        {
            UIUtils.colorize(colorButtonIcon, constColor, newChosenColor);
            this.constColor = newChosenColor;

            Stack<Transform> stack = new Stack<Transform>(); // buggy
            stack.Push(colorButton.uiItem.gameObject.transform);

            colorButton.uiItem.gameObject.DestroyGameObjectImmediate();
            UIStyle btnStyle = UIUtils.createImageButtonStyle(colorButtonIcon);
            colorButton = new DialogGUIButton("", delegate { colorEditClick(this.constColor); }, null, 32, 32, false, btnStyle);
            colorButton.image = btnStyle.normal.background;

            
            colorButton.Create(ref stack, HighLogic.UISkin);
        }
    }
}
