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
        private string briefMessage;

        public ConstellationControlDialog(string title, string briefMessage) : base(title, 
                                                                                    0.7f,                        //x
                                                                                    0.3f,                        //y
                                                                                    (int)(Screen.width*0.4),     //width
                                                                                    (int)(Screen.height*0.4),    //height
                                                                                    false)                       //close button
        {
            this.briefMessage = briefMessage;
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            //Label for the brief message
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.briefMessage, false, false) }));

            //A GUILayout for each row (eg label + textfield + button)
            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();
            for (int i=0; i< 5; i++)
            {
                DialogGUITextInput nameInput = new DialogGUITextInput("Constellation nickname", false, 50, null);
                DialogGUITextInput frequencyInput = new DialogGUITextInput(""+settings.PublicRadioFrequency, false, 4, null);
                DialogGUIButton colorButton = new DialogGUIButton("Color", a, 50, 50, false);
                DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);

                DialogGUIHorizontalLayout lineGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { colorButton, nameInput, frequencyInput, updateButton });
                eachRowGroupList.Add(lineGroup);
            }

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 15, 0, 0), TextAnchor.UpperLeft, rows)));

            return listComponments;
        }

        private void a()
        {
            CNCLog.Verbose("Color button is clicked but which row?");
        }

        protected override bool runIntenseInfo()
        {
            return true;
        }

        private void updateClick()
        {
            CNCLog.Verbose("Update button is clicked but which row?");
        }

        private void colorSelected(bool arg1)
        {
            CNCLog.Verbose("Color button is clicked but which row?");
        }

    }
}
