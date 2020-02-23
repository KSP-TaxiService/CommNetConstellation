using System;
using System.Collections.Generic;
using CommNet;
using UnityEngine;
using TMPro;
using CommNetConstellation.CommNetLayer;
using UnityEngine.UI;
using KSP.Localization;

namespace CommNetConstellation.UI.VesselMgtTools
{
    public class VanillaFreqTool : AbstractMgtTool
    {
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private DialogGUITextInput frequencyInput;
        private DialogGUIImage constellationColorImage;
        private bool membershipOption;

        public VanillaFreqTool(CommNetVessel thisVessel, Callback updateFreqRowsCallback) : base(thisVessel, "vanilla", Localizer.Format("#CNC_ToolsNames_MasterFrequency"), new List<Callback>() { updateFreqRowsCallback })//"Master frequency"
        {
        }

        public override List<DialogGUIBase> getContentComponents()
        {
            List<DialogGUIBase> layout = new List<DialogGUIBase>();

            DialogGUILabel msgLbl = new DialogGUILabel(Localizer.Format("#CNC_getContentCompon_msgLabel4"), 100, 32);//"Set up the master frequency in one go. All antennas will be assigned to this frequency, and Comm powers of those deployed antennas will be combined."
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            DialogGUILabel freqLabel = new DialogGUILabel("<b>" + Localizer.Format("#CNC_Generic_FrequencyLabel") + "</b>", 52, 12);//Frequency
            frequencyInput = new DialogGUITextInput(CNCSettings.Instance.PublicRadioFrequency+"", false, CNCSettings.MaxDigits, setConstellFreq, 45, 25);
            DialogGUIToggle membershipToggle = new DialogGUIToggle(false, "", membershipFlagToggle);
            DialogGUILabel membershipLabel = new DialogGUILabel("<b>"+Localizer.Format("#CNC_getContentCompon_membershipLabel") +"</b>", 200, 12);//Talk to constellation members only

            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(20), membershipToggle, membershipLabel });
            layout.Add(constellationGroup);

            constellationColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, Color.white, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(getConstellationName, 200, 12);
            layout.Add(new DialogGUIHorizontalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { constNameLabel, constellationColorImage }));

            DialogGUIButton updateButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_UpdateButton"), updateClick, false);//"Update"
            DialogGUIButton publicButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_PublicButton"), defaultClick, false);//"Revert to public"
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, publicButton, new DialogGUIFlexibleSpace() });
            layout.Add(actionGroup);

            return layout;
        }

        private void membershipFlagToggle(bool state)
        {
            this.membershipOption = state;
        }

        private string setConstellFreq(string freqInput)
        {
            //do nothing
            return freqInput;
        }

        private string getConstellationName()
        {
            try
            {
                Constellation thisConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text));

                if (thisConstellation != null)
                {
                    constellationColorImage.uiItem.GetComponent<RawImage>().color = thisConstellation.color;
                    return "<b>"+Localizer.Format("#CNC_getConstellationName_Constellation") +"</b>: " + thisConstellation.name;//Constellation
                }
            }
            catch (Exception e) { }

            constellationColorImage.uiItem.GetComponent<RawImage>().color = Color.clear;
            return Localizer.Format("#CNC_getConstellationName_Unrecognised");//"<b>Constellation</b>: Unrecognised"
        }

        private void defaultClick()
        {
            frequencyInput.uiItem.GetComponent<TMP_InputField>().text = CNCSettings.Instance.PublicRadioFrequency.ToString();
        }

        private void updateClick()
        {
            try
            {
                try
                {
                    short userFreq = short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text);

                    //Check frequency
                    if (userFreq < 0)
                    {
                        throw new Exception(Localizer.Format("#CNC_CheckFrequency_negative"));//"Frequency cannot be negative"
                    }
                    else if (!Constellation.isFrequencyValid(userFreq))
                    {
                        throw new Exception(Localizer.Format("#CNC_CheckFrequency_Valid", short.MaxValue));//"Frequency must be between 0 and " + 
                    }

                    //ALL OK
                    if (!this.cncVessel.isFreqListEditable())
                    {
                        return;
                    }

                    //update all antennas to new freq
                    for (int i=0; i< this.antennas.Count; i++)
                    {
                        CNCAntennaPartInfo thisAntenna = this.antennas[i];
                        this.cncVessel.toggleAntenna(thisAntenna, true);
                        if (thisAntenna.frequency != userFreq) // update each antenna to user freq
                            this.cncVessel.updateFrequency(thisAntenna, userFreq);
                    }
                    this.cncVessel.rebuildFreqList();

                    //if membership option is not enabled, add public freq of same comm power
                    if (!membershipOption && userFreq != CNCSettings.Instance.PublicRadioFrequency)
                    {
                        double commPower = this.cncVessel.getMaxComPower(userFreq);
                        this.cncVessel.addToFreqList(CNCSettings.Instance.PublicRadioFrequency, commPower);
                    }
                    
                    actionCallbacks[0]();
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
