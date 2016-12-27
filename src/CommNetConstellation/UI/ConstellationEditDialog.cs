using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CommNetConstellation.UI
{
    public class ConstellationEditDialog : AbstractDialog
    {
        public int textureWidth = 360;
        public int textureHeight = 120;
        private float saturationSlider = 0.0F;
        private Texture2D saturationTexture;

        private string description = "You are editing this constellation.\n\n";
        private static readonly Texture2D colorTexture = CNCUtils.loadImage("colorDisplay");

        public ConstellationEditDialog(string dialogTitle, Constellation thisConstellation) : base(dialogTitle,
                                                                                                        0.5f,                               //x
                                                                                                        0.5f,                               //y
                                                                                                        250,                                //width
                                                                                                        255,                                //height
                                                                                                        new string[] { "showclosebutton" }) //arguments
        {

        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description, false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 40, 12);
            DialogGUITextInput nameInput = new DialogGUITextInput("Some Constellation Name", false, CNCSettings.Instance.MaxNumChars, null, 129, 32);

            DialogGUIHorizontalLayout lineGroup3 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameLabel, nameInput, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup3);

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Radio frequency</b>", 50, 24);
            DialogGUITextInput frequencyInput = new DialogGUITextInput("12345", false, 5, null, 40, 32);
            DialogGUILabel colorLabel = new DialogGUILabel("<b>Color</b>", 30, 12);
            DialogGUIButton colorButton = new DialogGUIButton(Sprite.Create(colorTexture, new Rect(0, 0, 32, 32), Vector2.zero), delegate { }, 32, 32, false); colorButton.width = 32; colorButton.height = 32;

            DialogGUIHorizontalLayout lineGroup1 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(18), colorLabel, colorButton });
            listComponments.Add(lineGroup1);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);

            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup2);

            DialogGUILabel messageLabel = new DialogGUILabel("Message: <color=#dc3e44>The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.</color>", true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(false, false, 4, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        protected override bool runIntenseInfo(object[] args)
        {
            return true;
        }

        private void updateClick()
        {

        }
    }
}
