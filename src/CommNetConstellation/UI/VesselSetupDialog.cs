using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;

namespace CommNetConstellation.UI
{
    public class VesselSetupDialog : AbstractDialog
    {
        private Vessel hostVessel;
        private CNConstellationModule hostModule;
        private string description = "You are editing ";
        private static readonly Texture2D colorTexture = CNCUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel thisVessel, CNConstellationModule thisModule) : base(title, 
                                                                                                            0.7f,                               //x
                                                                                                            0.5f,                               //y
                                                                                                            250,                                //width
                                                                                                            220,                                //height
                                                                                                            new string[] { "showclosebutton" }) //arguments
        {
            this.hostVessel = thisVessel; // could be null (in editor)
            this.hostModule = thisModule;

            if (this.hostVessel != null)
                this.description += string.Format("'{0}'.\n\n", this.hostVessel.vesselName);
            else
                this.description += "this vessel under construction.\n\n";
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description, false, false) }));

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Radio frequency</b>", 50, 32);
            DialogGUITextInput frequencyInput = new DialogGUITextInput("12345", false, 5, null, 32, 32);
            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32,32), Vector2.zero, Color.yellow, colorTexture);

            DialogGUIHorizontalLayout lineGroup1 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUIFlexibleSpace(), colorImage, new DialogGUIFlexibleSpace() });
            listComponments.Add(lineGroup1);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultClick, false);

            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { updateButton, publicButton });
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
