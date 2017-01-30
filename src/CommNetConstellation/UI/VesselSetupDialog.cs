using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Smooth.Algebraics;

namespace CommNetConstellation.UI
{
    public class VesselSetupDialog : AbstractDialog
    {
        private Part rightClickedPart = null;
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";
        private string message = "Ready";

        private Callback<Vessel, short> updateCallback;

        private short conFreq = 0;
        private DialogGUITextInput frequencyInput;

        private DialogGUIImage colorImage;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel vessel, Part cmdPart, Callback<Vessel, short>  updateCallback) : base(title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                250, //width
                                                                                                                255, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.rightClickedPart = cmdPart;
            this.updateCallback = updateCallback;

            if (this.rightClickedPart != null) // first choice
            {
                this.description = string.Format("You are editing this command part '{0}'.", this.rightClickedPart.partInfo.title);
                CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                this.conFreq = cncModule.radioFrequency;
            }
            else if (this.hostVessel != null)
            {
                this.description = string.Format("You are editing the whole vessel '{0}' (overriding <b>all</b> command parts).", this.hostVessel.vesselName);
                CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                this.conFreq = cv.getRadioFrequency();
            }
        }

        private string getMessage()
        {
            return "Message: " + this.message;
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            frequencyInput = new DialogGUITextInput(conFreq.ToString(), false, 5, setConFreq, 45, 25);
            DialogGUIHorizontalLayout freqGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(100)});
            listComponments.Add(freqGroup);

            colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, Color.white, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(getConstellationName, 200, 12);
            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { constNameLabel, colorImage });
            listComponments.Add(constellationGroup);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultClick, false);

            DialogGUIHorizontalLayout lineGroup2 = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { updateButton, publicButton });
            listComponments.Add(lineGroup2);

            DialogGUILabel messageLabel = new DialogGUILabel(getMessage, true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(false, false, 4, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        private string setConFreq(string newFreqStr)
        {
            try
            {
                short newFreq = short.Parse(newFreqStr);

                if (newFreq < 0)
                {
                    message = "<color=red>This frequency cannot be negative</color>";
                    return newFreqStr;
                }
                else
                {
                    this.conFreq = newFreq;
                    return this.conFreq.ToString();
                }
            }
            catch (FormatException e)
            {
                message = "<color=red>This frequency must be numeric only</color>";
                return newFreqStr;
            }
            catch (OverflowException e)
            {
                message = string.Format("<color=red>This frequency must be equal to or less than {0}</color>", short.MaxValue);
                return newFreqStr;
            }
        }

        private string getConstellationName()
        {
            Constellation thisConstellation = Constellation.find(CNCCommNetScenario.Instance.constellations, conFreq);

            if (thisConstellation != null)
            {
                colorImage.uiItem.GetComponent<RawImage>().color = thisConstellation.color;
                return "<b>Under:</b> " + thisConstellation.name;
            }

            colorImage.uiItem.GetComponent<RawImage>().color = Color.clear;
            return "<b>Under:</b> Unrecognised";
        }

        private void updateClick()
        {
            //Check errors
            if (!CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == this.conFreq))
            {
                message = "<color=red>Please choose one of the existing constellations</color>";
                return;
            }
            else if (!Constellation.isFrequencyValid(this.conFreq))
            {
                message = string.Format("<color=red>This frequency must be between 0 and {0}</color>", short.MaxValue);
                return;
            }

            if (rightClickedPart != null) // either in editor or flight
            {
                if(this.hostVessel != null) // flight
                {
                    CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                    cv.updateRadioFrequency(this.conFreq, rightClickedPart);
                }
                else // editor
                {
                    CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                    cncModule.radioFrequency = this.conFreq;
                }

                message = "The frequency of this command part is updated";
            }
            else if (this.hostVessel != null) // tracking station
            {
                CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                short prevFrequency = cv.getRadioFrequency();
                cv.updateRadioFrequency(this.conFreq);
                message = "All individual frequencies in this entire vessel are updated to this frequency";

                updateCallback(this.hostVessel, prevFrequency);
            }
            else
            {
                message = "Something is broken ;_;";
            }
        }

        private void defaultClick()
        {
            this.conFreq = CNCSettings.Instance.PublicRadioFrequency;
            updateClick();
            frequencyInput.SetOptionText(this.conFreq.ToString()); // TODO: no effect?
            message = "Reverted to the public constellation";
        }
    }
}
