using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit the constellation membership of this vessel (Controller)
    /// </summary>
    public class VesselSetupDialog : AbstractDialog
    {
        private Part rightClickedPart = null;
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";
        private string message = "Ready";

        private Callback<Vessel, short> updateCallback;

        private short inputFreq = 0;
        private DialogGUITextInput frequencyInput;

        private bool membershipFlag = false;
        private DialogGUIToggle membershipToggle;

        private DialogGUIImage constellationColorImage;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel vessel, Part cmdPart, Callback<Vessel, short>  updateCallback) : base(title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                250, //width
                                                                                                                295, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.rightClickedPart = cmdPart;
            this.updateCallback = updateCallback;

            if (this.rightClickedPart != null) // first choice
            {
                this.description = string.Format("You are editing this command part '{0}'.", this.rightClickedPart.partInfo.title);
                CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                this.inputFreq = cncModule.radioFrequency;
                this.membershipFlag = cncModule.communicationMembershipFlag;
            }
            else if (this.hostVessel != null)
            {
                this.description = string.Format("You are editing the whole vessel '{0}' (overriding <b>all</b> command parts).", this.hostVessel.vesselName);
                CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                this.inputFreq = cv.getRadioFrequency();
                this.membershipFlag = cv.getMembershipFlag();
            }
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description+"\n\n", false, false) }));

            DialogGUILabel freqLabel = new DialogGUILabel("<b>Frequency</b>", 52, 12);
            frequencyInput = new DialogGUITextInput(inputFreq.ToString(), false, CNCSettings.MaxDigits, setConstellFreq, 45, 25);
            DialogGUIHorizontalLayout freqGRoup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, new DialogGUISpace(100) });
            listComponments.Add(freqGRoup);

            constellationColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, Color.white, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(getConstellationName, 200, 12);
            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { constNameLabel, constellationColorImage });
            listComponments.Add(constellationGroup);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultClick, false);
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { updateButton, publicButton });
            listComponments.Add(actionGroup);

            membershipToggle = new DialogGUIToggle(this.membershipFlag, "<b>Talk to constellation members only</b>", membershipFlagToggle);
            listComponments.Add(membershipToggle);

            DialogGUILabel messageLabel = new DialogGUILabel(getStatusMessage, true, false);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, false, new DialogGUIVerticalLayout(false, false, 4, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { messageLabel })));

            return listComponments;
        }

        /// <summary>
        /// For the dialog's status box to display
        /// </summary>
        private string getStatusMessage()
        {
            return "Message: " + this.message;
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setConstellFreq(string newFreqStr)
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
                    this.inputFreq = newFreq;
                    return this.inputFreq.ToString();
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

        /// <summary>
        /// For the dialog to get the constellation name based on the frequency in the textfield
        /// </summary>
        private string getConstellationName()
        {
            Constellation thisConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == inputFreq);

            if (thisConstellation != null)
            {
                constellationColorImage.uiItem.GetComponent<RawImage>().color = thisConstellation.color;
                return "<b>Constellation:</b> " + thisConstellation.name;
            }

            constellationColorImage.uiItem.GetComponent<RawImage>().color = Color.clear;
            //message = "The constellation associated with this frequency is <color=red>not found</color>. Please choose one of the existing constellations";
            return "<b>Constellation:</b> Unrecognised";
        }

        /// <summary>
        /// Action to update the frequency of the vessel
        /// </summary>
        private void updateClick()
        {
            //Check errors
            if (!CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == this.inputFreq))
            {
                message = "<color=red>Please choose one of the existing constellations</color>";
                return;
            }
            else if (!Constellation.isFrequencyValid(this.inputFreq))
            {
                message = string.Format("<color=red>This frequency must be between 0 and {0}</color>", short.MaxValue);
                return;
            }

            if (rightClickedPart != null) // either in editor or flight
            {
                if(this.hostVessel != null) // flight
                {
                    CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                    cv.updateRadioFrequency(this.inputFreq, rightClickedPart);
                }
                else // editor
                {
                    CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                    cncModule.radioFrequency = this.inputFreq;
                }

                message = "The frequency of this command part is updated";
            }
            else if (this.hostVessel != null) // tracking station
            {
                CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                short prevFrequency = cv.getRadioFrequency();
                cv.updateRadioFrequency(this.inputFreq);
                message = "All individual frequencies in this entire vessel are updated to this frequency";

                updateCallback(this.hostVessel, prevFrequency);
            }
            else
            {
                message = "Something is broken ;_;";
            }
        }

        /// <summary>
        /// Action to reset the vessel's frequency to the public one
        /// </summary>
        private void defaultClick()
        {
            this.inputFreq = CNCSettings.Instance.PublicRadioFrequency;
            updateClick();
            frequencyInput.SetOptionText(this.inputFreq.ToString());
            message = "Reverted to the public constellation";
        }

        /// <summary>
        /// Action to toggle the vessel's membership flag
        /// </summary>
        private void membershipFlagToggle(bool flag)
        {
            if (rightClickedPart != null) // either in editor or flight
            {
                if (this.hostVessel != null) // flight
                {
                    CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                    cv.updateMembershipFlag(flag, rightClickedPart);
                }
                else // editor
                {
                    CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                    cncModule.communicationMembershipFlag = flag;
                }

                message = "The communication membership of this command part is updated";
            }
            else if (this.hostVessel != null) // tracking station
            {
                CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                cv.updateMembershipFlag(flag);
                message = "All individual membership flags in this entire vessel are updated";
            }
            else
            {
                message = "Something is broken ;_;";
            }
        }
    }
}
