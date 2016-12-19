using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace CommNetConstellation.UI
{
    public class ConstellationControlDialog : AbstractDialog
    {
        public ConstellationControlDialog(string title) : base(title, 
                                                            0.7f,                           //x
                                                            0.5f,                           //y
                                                            (int)(Screen.width*0.3),        //width
                                                            (int)(Screen.height*0.5),       //height
                                                            new string[] { "showversion" }) //arguments
        {
            
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();
            
            setupConstellationList(listComponments);
            setupSatelliteList(listComponments);

            return listComponments;
        }

        private void setupConstellationList(List<DialogGUIBase> listComponments)
        {
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel("<b>You can manage multiple constellations of vessels</b>", false, false) }));

            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();
            for (int i = 0; i < 5; i++)
            {
                DialogGUITextInput nameInput = new DialogGUITextInput("Constellation nickname", false, 50, null);
                DialogGUITextInput frequencyInput = new DialogGUITextInput("" + settings.PublicRadioFrequency, false, 5, null);
                DialogGUIButton colorButton = new DialogGUIButton("Color", a, false);
                DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
                DialogGUIButton deleteButton = new DialogGUIButton("Remove", deleteClick, false);

                DialogGUIHorizontalLayout lineGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { colorButton, frequencyInput, nameInput, updateButton, deleteButton });
                eachRowGroupList.Add(lineGroup);
            }

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 15, 0, 0), TextAnchor.UpperLeft, rows)));
        }

        private void setupSatelliteList(List<DialogGUIBase> listComponments)
        {
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel("\n<b>You can edit the constellation configuration of each eligible vessel</b>", false, false) }));

            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();
            for (int i = 0; i < 5; i++)
            {
                DialogGUIButton vesselButton = new DialogGUIButton("VesselName", vesselFocusClick, false);
                DialogGUILabel locationLabel = new DialogGUILabel("@ PLANET", false, false);
                DialogGUIButton setupButton = new DialogGUIButton("Setup", vesselSetupClick, false);

                DialogGUIHorizontalLayout lineGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { vesselButton, locationLabel, setupButton });
                eachRowGroupList.Add(lineGroup);
            }

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 15, 0, 0), TextAnchor.UpperLeft, rows)));
        }

        private void a()
        {
            CNCLog.Verbose("Color button is clicked but which row?");
        }

        protected override bool runIntenseInfo(System.Object[] args)
        {
            return true;
        }

        private void updateClick()
        {
            CNCLog.Verbose("Update button is clicked but which row?");
        }

        private void deleteClick()
        {

        }

        private void vesselSetupClick()
        {

        }

        private void vesselFocusClick()
        {

        }

        private void colorSelected(bool arg1)
        {
            CNCLog.Verbose("Color button is clicked but which row?");
        }

    }
}
