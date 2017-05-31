using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

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

        private Callback<Vessel, short> updateCallback;
        private DialogGUITextInput frequencyInput;
        private DialogGUIToggle membershipToggle;

        private DialogGUIImage constellationColorImage;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel vessel, Part cmdPart, Callback<Vessel, short>  updateCallback) : base("vesselEdit",
                                                                                                                title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                250, //width
                                                                                                                240, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.rightClickedPart = cmdPart;
            this.updateCallback = updateCallback;

            if (this.rightClickedPart != null) // first choice
                this.description = string.Format("You are editing this command part '{0}'.", this.rightClickedPart.partInfo.title);
            else if (this.hostVessel != null)
                this.description = string.Format("You are editing the whole vessel '{0}' (overriding <b>all</b> command parts).", this.hostVessel.vesselName);
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            short inputFreq = CNCSettings.Instance.PublicRadioFrequency;
            bool membershipFlag = false;

            if (this.rightClickedPart != null) // first choice
            {
                CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                inputFreq = cncModule.radioFrequency;
                membershipFlag = cncModule.communicationMembershipFlag;
            }
            else if (this.hostVessel != null)
            {
                CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                inputFreq = cv.getRadioFrequency();
                membershipFlag = cv.getMembershipFlag();
            }

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

            membershipToggle = new DialogGUIToggle(membershipFlag, "<b>Talk to constellation members only</b>", membershipFlagToggle);
            listComponments.Add(membershipToggle);

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateClick, false);
            DialogGUIButton publicButton = new DialogGUIButton("Revert to public", defaultClick, false);
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { updateButton, publicButton });
            listComponments.Add(actionGroup);

            return listComponments;
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setConstellFreq(string newFreqStr)
        {
            try
            {
                try // input validations
                {
                    short newFreq = short.Parse(newFreqStr);

                    if (newFreq < 0)
                        throw new Exception("Frequency cannot be negative");
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
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }

            return newFreqStr;
        }

        /// <summary>
        /// For the dialog to get the constellation name based on the frequency in the textfield
        /// </summary>
        private string getConstellationName()
        {
            try
            {
                Constellation thisConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text));

                if (thisConstellation != null)
                {
                    constellationColorImage.uiItem.GetComponent<RawImage>().color = thisConstellation.color;
                    return "<b>Constellation:</b> " + thisConstellation.name;
                }
            } catch (Exception e) { }

            constellationColorImage.uiItem.GetComponent<RawImage>().color = Color.clear;
            return "<b>Constellation:</b> Unrecognised";
        }

        /// <summary>
        /// Action to update the frequency of the vessel
        /// </summary>
        private void updateClick()
        {
            try
            {
                short inputFreq = short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text);

                //Check errors
                if (!CNCCommNetScenario.Instance.constellations.Any(x => x.frequency == inputFreq))
                {
                    throw new Exception("Please choose an existing constellation");
                }
                else if (!Constellation.isFrequencyValid(inputFreq))
                {
                    throw new Exception("Frequency must be between 0 and "+short.MaxValue);
                }

                if (rightClickedPart != null) // either in editor or flight
                {
                    if (this.hostVessel != null) // flight
                    {
                        CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                        cv.updateRadioFrequency(inputFreq, rightClickedPart);
                    }
                    else // editor
                    {
                        CNConstellationModule cncModule = rightClickedPart.FindModuleImplementing<CNConstellationModule>();
                        cncModule.radioFrequency = inputFreq;
                    }

                    string message = string.Format("Frequency of {0} is updated to {1}", rightClickedPart.partInfo.title, inputFreq);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));
                }
                else if (this.hostVessel != null) // tracking station
                {
                    CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                    short prevFrequency = cv.getRadioFrequency();
                    cv.updateRadioFrequency(inputFreq);

                    string message = string.Format("Individual frequencies of {0} are updated to {1}", this.hostVessel.GetName(), inputFreq);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));

                    updateCallback(this.hostVessel, prevFrequency);
                }
                else
                {
                    throw new Exception("Something is broken ;_;");
                }
            }
            catch (Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>" + e.Message + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }
        }

        /// <summary>
        /// Action to reset the vessel's frequency to the public one
        /// </summary>
        private void defaultClick()
        {
            frequencyInput.uiItem.GetComponent<TMP_InputField>().text = CNCSettings.Instance.PublicRadioFrequency.ToString();
            updateClick();            
        }

        /// <summary>
        /// Action to toggle the vessel's membership flag
        /// </summary>
        private void membershipFlagToggle(bool flag)
        {
            try
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

                    string message = string.Format("Talk membership of {0} is updated to {1}", rightClickedPart.partInfo.title , flag);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));
                }
                else if (this.hostVessel != null) // tracking station
                {
                    CNCCommNetVessel cv = hostVessel.Connection as CNCCommNetVessel;
                    cv.updateMembershipFlag(flag);

                    string message = string.Format("Individual talk memberships of {0} are updated to {1}", this.hostVessel.GetName(), flag);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(message, CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT));
                }
                else
                {
                    throw new Exception("Something is broken ;_;");
                }
            }
            catch(Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>"+e.Message+"</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }
        }
    }
}
