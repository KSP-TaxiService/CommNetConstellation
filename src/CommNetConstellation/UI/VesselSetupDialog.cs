using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;

namespace CommNetConstellation.UI
{
    public class VesselSetupDialog : AbstractDialog
    {
        private Vessel hostVessel;
        private string description = "You are editing ";
        private static readonly Texture2D colorTexture = CNCUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel thisVessel) : base(title, 
                                                                            0.5f, //x
                                                                            0.5f, //y
                                                                            250, //width
                                                                            255, //height
                                                                            new string[] { "showclosebutton" }) //arguments
        {
            this.hostVessel = thisVessel; // could be null (in editor)

            if (this.hostVessel != null)
                this.description += string.Format("'{0}'.\n\n", this.hostVessel.vesselName);
            else
                this.description += "this vessel under construction.\n\n";
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description, false, false) }));

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Radio frequency</b>", 32, 24);
             DialogGUITextInput frequencyInput = new DialogGUITextInput("12345", false, 5, null, 20, 32);

            DialogGUIHorizontalLayout lineGroup1 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(50) , new DialogGUIFlexibleSpace()});
            listComponments.Add(lineGroup1);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, Color.yellow, colorTexture); colorImage.width = 32; colorImage.height = 32;
            DialogGUILabel constNameLabel = new DialogGUILabel("<b>Under:</b> Some Name", 200, 12);

            DialogGUIHorizontalLayout lineGroup3 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { constNameLabel, colorImage });
            listComponments.Add(lineGroup3);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultClick, false);

            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { updateButton, publicButton });
            listComponments.Add(lineGroup2);

            DialogGUILabel messageLabel = new DialogGUILabel("Message: <color=#dc3e44>The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.</color>", true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(false, false, 4, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        protected override bool runIntenseInfo(System.Object[] args)
        {
            return true;
        }

        private void updateClick()
        {

        }

        private void defaultClick()
        {

        }
    }
}
