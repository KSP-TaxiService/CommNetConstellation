using System.Collections.Generic;
using CommNet;
using UnityEngine;
using CommNetConstellation.CommNetLayer;
using System;
using TMPro;
using KSP.Localization;

namespace CommNetConstellation.UI.VesselMgtTools
{
    public class IndepAntennaFreqTool : AbstractMgtTool
    {
        private UIStyle style;
        private DialogGUITextInput[] freqInputArray;

        public IndepAntennaFreqTool(CommNetVessel thisVessel, Callback updateFreqRowsCallback) : base(thisVessel, "antenna2", Localizer.Format("#CNC_ToolsNames_AntennaConfigs"), new List<Callback>() { updateFreqRowsCallback })//"Antenna Configs"
        {
            this.style = new UIStyle();
            this.style.alignment = TextAnchor.MiddleLeft;
            this.style.fontStyle = FontStyle.Normal;
            this.style.normal = new UIStyleState();
            this.style.normal.textColor = Color.white;
        }

        public override List<DialogGUIBase> getContentComponents()
        {
            List<DialogGUIBase> layout = new List<DialogGUIBase>();

            DialogGUILabel msgLbl = new DialogGUILabel(Localizer.Format("#CNC_getContentCompon_msgLabel2"), 100, 16);//"Change the frequency of each antenna."
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            freqInputArray = new DialogGUITextInput[antennas.Count];

            DialogGUIVerticalLayout nameColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout useColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout freqColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout freqInputColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout updateColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            for (int i = 0; i < antennas.Count; i++)
            {
                int index = i;//convert to solid-reference variable for delegate block
                CNCAntennaPartInfo antennaInfo = antennas[i];

                DialogGUILabel nameLabel = new DialogGUILabel(Localizer.Format("#CNC_getContentCompon_nameLabel", antennaInfo.name), style); nameLabel.size = new Vector2(200, 32);//string.Format("Name: {0}",)
                DialogGUILabel usageLabel = new DialogGUILabel(Localizer.Format("#CNC_getContentCompon_usageLabel") + ": " + (antennaInfo.inUse ? "<color=green>"+Localizer.Format("#CNC_Generic_Yes") +"</color>" : "<color=red>"+Localizer.Format("#CNC_Generic_No") +"</color>"), style); usageLabel.size = new Vector2(75, 32);//In UseYesNo
                DialogGUILabel freqLabel = new DialogGUILabel(Localizer.Format("#CNC_Generic_FrequencyLabel"), style); freqLabel.size = new Vector2(65, 32);//"Frequency"
                freqInputArray[i] = new DialogGUITextInput(antennaInfo.frequency.ToString(), false, CNCSettings.MaxDigits, setAntennaFreq, 65, 25);
                DialogGUIButton updateButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_UpdateButton"), delegate { updateAntennaFreq(antennaInfo, freqInputArray[index].uiItem.GetComponent<TMP_InputField>().text); }, 70, 25, false);//"Update"

                nameColumn.AddChild(nameLabel);
                useColumn.AddChild(usageLabel);
                freqColumn.AddChild(freqLabel);
                freqInputColumn.AddChild(new DialogGUISpace(3));
                freqInputColumn.AddChild(freqInputArray[index]);
                freqInputColumn.AddChild(new DialogGUISpace(4));
                updateColumn.AddChild(new DialogGUISpace(3));
                updateColumn.AddChild(updateButton);
                updateColumn.AddChild(new DialogGUISpace(4));
            }

            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { nameColumn, useColumn, freqColumn, freqInputColumn, updateColumn }));

            return layout;
        }

        private string setAntennaFreq(string newFreqStr)
        {
            //do nothing
            return newFreqStr;
        }

        private void updateAntennaFreq(CNCAntennaPartInfo antennaInfo, string freqString)
        {
            try
            {
                try
                {
                    short inputFreq = short.Parse(freqString);

                    //Check frequency
                    if (inputFreq < 0)
                    {
                        throw new Exception(Localizer.Format("#CNC_CheckFrequency_negative"));//"Frequency cannot be negative"
                    }
                    else if (!GameUtils.NonLinqAny(CNCCommNetScenario.Instance.constellations, inputFreq))
                    {
                        throw new Exception(Localizer.Format("#CNC_CheckFrequency_Exist"));//"Please choose an existing constellation"
                    }
                    else if (!Constellation.isFrequencyValid(inputFreq))
                    {
                        throw new Exception(Localizer.Format("#CNC_CheckFrequency_Valid", short.MaxValue));//"Frequency must be between 0 and " + 
                    }

                    if (base.cncVessel != null && antennaInfo != null && inputFreq >= 0)
                    {
                        base.cncVessel.updateFrequency(antennaInfo, inputFreq);
                        base.cncVessel.OnAntennaChange();
                        this.selfRefresh();
                        actionCallbacks[0]();
                    }
                }
                catch (FormatException e)
                {
                    throw new FormatException(Localizer.Format("#CNC_CheckFrequency_Format"));//"Frequency must be numeric only"
                }
                catch (OverflowException e)
                {
                    throw new OverflowException(Localizer.Format("#CNC_CheckFrequency_Overflow", short.MaxValue));//string.Format("Frequency must be equal to or less than {0}", )
                }
            }
            catch (Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                ScreenMessages.PostScreenMessage(msg);
            }
        }
    }
}
