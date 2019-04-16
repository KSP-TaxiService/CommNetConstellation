using System.Collections.Generic;
using CommNet;
using UnityEngine;
using CommNetConstellation.CommNetLayer;
using System;
using TMPro;

namespace CommNetConstellation.UI.VesselMgtTools
{
    public class IndepAntennaFreqTool : AbstractMgtTool
    {
        private UIStyle style;
        private DialogGUITextInput[] freqInputArray;

        public IndepAntennaFreqTool(CommNetVessel thisVessel, Callback updateFreqRowsCallback) : base(thisVessel, "antenna2", "Antenna Configs", new List<Callback>() { updateFreqRowsCallback })
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

            DialogGUILabel msgLbl = new DialogGUILabel("Change the frequency of each antenna.", 100, 32);
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            DialogGUIVerticalLayout rows = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            freqInputArray = new DialogGUITextInput[antennas.Count];

            for (int i = 0; i < antennas.Count; i++)
            {
                int index = i;//convert to solid-reference variable for delegate block
                CNCAntennaPartInfo antennaInfo = antennas[i];

                DialogGUIHorizontalLayout rowLayout = new DialogGUIHorizontalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

                DialogGUILabel nameLabel = new DialogGUILabel(string.Format("Name: {0}",antennaInfo.name), style); nameLabel.size = new Vector2(200, 32);
                DialogGUILabel usageLabel = new DialogGUILabel("In Use: " + (antennaInfo.inUse ? "<color=green>Yes</color>" : "<color=red>No</color>"), style); usageLabel.size = new Vector2(75, 32);
                DialogGUILabel freqLabel = new DialogGUILabel("Frequency", style); freqLabel.size = new Vector2(65, 32);
                freqInputArray[i] = new DialogGUITextInput(antennaInfo.frequency.ToString(), false, CNCSettings.MaxDigits, setAntennaFreq, 65, 25);
                DialogGUIButton updateButton = new DialogGUIButton("Update", delegate { updateAntennaFreq(antennaInfo, freqInputArray[index].uiItem.GetComponent<TMP_InputField>().text); }, 70, 25, false);

                rowLayout.AddChild(nameLabel);
                rowLayout.AddChild(new DialogGUISpace(10));
                rowLayout.AddChild(usageLabel);
                rowLayout.AddChild(new DialogGUISpace(5));
                rowLayout.AddChild(freqLabel);
                rowLayout.AddChild(freqInputArray[index]);
                rowLayout.AddChild(new DialogGUISpace(5));
                rowLayout.AddChild(updateButton);
                rows.AddChild(rowLayout);
            }

            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { rows }));

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
                        throw new Exception("Frequency cannot be negative");
                    }
                    else if (!GameUtils.NonLinqAny(CNCCommNetScenario.Instance.constellations, inputFreq))
                    {
                        throw new Exception("Please choose an existing constellation");
                    }
                    else if (!Constellation.isFrequencyValid(inputFreq))
                    {
                        throw new Exception("Frequency must be between 0 and " + short.MaxValue);
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
                    throw new FormatException("Frequency must be numeric only");
                }
                catch (OverflowException e)
                {
                    throw new OverflowException(string.Format("Frequency must be equal to or less than {0}", short.MaxValue));
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
