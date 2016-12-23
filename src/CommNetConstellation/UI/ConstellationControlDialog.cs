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
        private static readonly Texture2D colorTexture = CNCUtils.loadImage("colorDisplay");
        private static readonly Texture2D focusTexture = CNCUtils.loadImage("target");

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
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel("\n<b>You can manage multiple constellations of vessels</b>", false, false) }));
            
            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();

            DialogGUIButton createButton = new DialogGUIButton("New constellation", newConstellationClick, false);
            DialogGUIHorizontalLayout creationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), createButton, new DialogGUIFlexibleSpace() });
            eachRowGroupList.Add(creationGroup);

            for (int i = 0; i < 5; i++)
            {
                DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.zero, Color.yellow, colorTexture);
                DialogGUILabel freqLabel = new DialogGUILabel("Frequency: <color=#ff0000>12345</color>", false, false);
                DialogGUILabel vesselLabel = new DialogGUILabel("Some Constellation Name", false, false);
                DialogGUILabel numSatsLabel = new DialogGUILabel("123 vessels", false, false);
                DialogGUIButton updateButton = new DialogGUIButton("Edit", editConstellationClick, false);

                DialogGUIBase[] rowGUIBase = new DialogGUIBase[] { colorImage, vesselLabel, freqLabel, numSatsLabel, new DialogGUIFlexibleSpace(), updateButton, null, new DialogGUIFlexibleSpace() };
                if (i == 0)
                    rowGUIBase[rowGUIBase.Length - 2] = new DialogGUIButton("Reset", resetConstellationClick, false);
                else
                    rowGUIBase[rowGUIBase.Length - 2] = new DialogGUIButton("Remove", deleteConstellationClick, false);

                DialogGUIHorizontalLayout lineGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, rowGUIBase);
                eachRowGroupList.Add(lineGroup);
            }

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, rows)));
        }

        private void setupSatelliteList(List<DialogGUIBase> listComponments)
        {
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel("\n<b>You can edit the constellation configuration of each eligible vessel</b>", false, false) }));

            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();
            for (int i = 0; i < 5; i++)
            {
                DialogGUIButton focusButton = new DialogGUIButton(Sprite.Create(focusTexture, new Rect(0, 0, 16, 16), Vector2.zero), vesselFocusClick, 20, 20,false);
                DialogGUILabel vesselLabel = new DialogGUILabel("Some Vessel Name", false, false);
                DialogGUILabel freqLabel = new DialogGUILabel("Frequency: <color=#ff0000>12345</color>", false, false);
                DialogGUILabel locationLabel = new DialogGUILabel("Orbiting: KERBIN", false, false);
                DialogGUIButton setupButton = new DialogGUIButton("Setup", vesselSetupClick, false);

                DialogGUIHorizontalLayout rowGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { focusButton, vesselLabel, freqLabel, locationLabel, setupButton });
                eachRowGroupList.Add(rowGroup);
            }

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, rows)));
        }

        protected override bool runIntenseInfo(System.Object[] args)
        {
            return true;
        }

        private void resetConstellationClick()
        {

        }

        private void deleteConstellationClick()
        {

        }

        private void vesselSetupClick()
        {
            new VesselSetupDialog("Vessel - <color=#00ff00>Setup</color>", null, null).launch(new System.Object[] {});
        }

        private void vesselFocusClick()
        {

        }

        private void newConstellationClick()
        {
            new ConstellationEditDialog("Constellation - <color=#00ff00>New</color>", null).launch(new System.Object[] { });
        }

        private void editConstellationClick()
        {
            new ConstellationEditDialog("Constellation - <color=#00ff00>Edit</color>", null).launch(new System.Object[] { });
        }
    }
}
