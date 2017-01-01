using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CommNetConstellation.UI
{
    public class ConstellationEditDialog : AbstractDialog
    {
        private string description = "You are editing this constellation.\n\n";
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private Texture2D colorButtonIcon;

        public ConstellationEditDialog(string dialogTitle, Constellation thisConstellation) : base(dialogTitle,
                                                                                                        0.5f, //x
                                                                                                        0.5f, //y
                                                                                                        250, //width
                                                                                                        255, //height
                                                                                                        new DialogOptions[] {})
        {
            this.colorButtonIcon = UIUtils.createAndColorize(colorTexture, new Color(1f, 1f, 1f), new Color(1f, 0f, 0f));
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description, false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 40, 12);
            DialogGUITextInput nameInput = new DialogGUITextInput("Some Constellation Name", false, CNCSettings.Instance.MaxNumChars, null, 130, 32);

            DialogGUIHorizontalLayout lineGroup3 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameLabel, nameInput, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup3);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Radio frequency</b>", 50, 24);
            DialogGUITextInput frequencyInput = new DialogGUITextInput("12345", false, 5, null, 45, 32);
            DialogGUILabel colorLabel = new DialogGUILabel("<b>Color</b>", 32, 12);

            UIStyle btnStyle = UIUtils.createImageButtonStyle(colorButtonIcon);
            DialogGUIButton colorButton = new DialogGUIButton("", delegate { colorEditClick(Color.red); }, null, 32, 32, false, btnStyle);
            colorButton.image = btnStyle.normal.background;

            DialogGUIHorizontalLayout lineGroup1 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(21), colorLabel, colorButton });
            listComponments.Add(lineGroup1);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);

            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup2);

            DialogGUILabel messageLabel = new DialogGUILabel("Message: <color=#dc3e44>The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.</color>", true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(false, false, 4, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        private void updateClick()
        {

        }

        private void colorEditClick(Color chosenColor)
        {
            new ColorPickerDialog(chosenColor, userChooseColor).launch(new System.Object[] { });
        }

        public void userChooseColor(Color chosenColor)
        {
            CNCLog.Debug("User color: " + UIUtils.colorToHex(chosenColor));
        }
    }
}
