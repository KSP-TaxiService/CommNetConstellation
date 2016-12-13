using CommNetConstellation.CommNetLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;


namespace CommNetConstellation.UI
{
    public class VesselSetupDialog : AbstractDialog
    {
        private Vessel hostVessel;
        private CNConstellationModule hostModule;

        public VesselSetupDialog(string title, Vessel thisVessel, CNConstellationModule thisModule) : base(title, 
                                                                                                            0.7f,                        //x
                                                                                                            0.5f,                        //y
                                                                                                            250,     //width
                                                                                                            170,    //height
                                                                                                            true)                       //close button
        {
            this.hostVessel = thisVessel; // could be null (in editor)
            this.hostModule = thisModule;

            /*
            if (this.hostVessel != null)
                this.description += string.Format(" '{0}'", this.hostVessel.vesselName);
            else
                this.description += '.';
            */
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            //listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description, false, false) }));
            //listComponments.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Radio frequency</b>", false, false);
            DialogGUITextInput frequencyInput = new DialogGUITextInput("" + settings.PublicRadioFrequency, false, 5, null);
            DialogGUIButton colorButton = new DialogGUIButton("Color", null, false);

            DialogGUIHorizontalLayout lineGroup1 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { freqLabel, frequencyInput, colorButton });
            listComponments.Add(lineGroup1);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
            DialogGUIButton publicButton = new DialogGUIButton("Default to Public", defaultClick, false);

            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { updateButton, publicButton });
            listComponments.Add(lineGroup2);

            DialogGUILabel messageLabel = new DialogGUILabel("Message: <color=#dc3e44>U FAIL</color>", true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 5, 2, 2), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        protected override bool runIntenseInfo()
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
