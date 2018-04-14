using CommNetConstellation.CommNetLayer;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit the ground station (Controller)
    /// </summary>
    public class GroundStationEditDialog : AbstractDialog
    {
        private CNCCommNetHome hostStation;
        private string description = "Something";

        private List<short> freqListShown = new List<short>();
        private Color constellColor = Color.white;

        private DialogGUITextInput nameInput;
        private DialogGUITextInput frequencyInput;
        private Callback<string> updateCallback;
        private DialogGUIImage stationColorImage;
        private DialogGUIVerticalLayout frequencyRowLayout;

        private static readonly Texture2D groundstationTexture = UIUtils.loadImage("groundStationMark");
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public GroundStationEditDialog(string title, CNCCommNetHome thisStation, Callback<string> updateCallback) : base("gsEdit",
                                                                                                                title,
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                290, //width
                                                                                                                300, //height
                                                                                                                new DialogOptions[] { DialogOptions.HideCloseButton })
        {
            this.hostStation = thisStation;
            this.updateCallback = updateCallback;
            this.description = string.Format("You are editing the ground station '{0}'.", thisStation.stationName);
            this.freqListShown.AddRange(thisStation.Frequencies);
            this.freqListShown.Sort();
            this.constellColor = thisStation.Color;

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            updateCallback(this.hostStation.ID);
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description + "\n\n", false, false) }));

            DialogGUILabel nameLabel = new DialogGUILabel("<b>Name</b>", 80, 12);
            nameInput = new DialogGUITextInput(this.hostStation.stationName, false, CNCSettings.MaxLengthName, setNameInput, 145, 25);
            DialogGUIButton defaultButton = new DialogGUIButton("Reset", defaultNameClick, 40, 32, false);
            DialogGUIHorizontalLayout nameGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { nameLabel, nameInput, defaultButton });
            listComponments.Add(nameGroup);

            stationColorImage = new DialogGUIImage(new Vector2(16, 16), Vector2.zero, this.hostStation.Color, groundstationTexture);
            DialogGUIButton colorButton = new DialogGUIButton("Color", colorEditClick, null, 50, 32, false);
            DialogGUILabel freqLabel = new DialogGUILabel("<b>New frequency</b>", 80, 12);
            frequencyInput = new DialogGUITextInput("", false, CNCSettings.MaxDigits, setFreqInput, 50, 25);
            DialogGUIButton addButton = new DialogGUIButton("Add", addClick, 40, 32, false);
            DialogGUIHorizontalLayout freqGRoup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { freqLabel, frequencyInput, addButton, new DialogGUISpace(5), colorButton, stationColorImage });
            listComponments.Add(freqGRoup);

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[this.hostStation.Frequencies.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < this.freqListShown.Count; i++)
            {
                rows[i + 1] = createConstellationRow(this.freqListShown[i]);
            }

            frequencyRowLayout = new DialogGUIVerticalLayout(10, 100, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, rows);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, frequencyRowLayout));

            DialogGUIButton updateButton = new DialogGUIButton("Update", updateAction, false);
            DialogGUIButton cancelButton = new DialogGUIButton("Cancel", delegate { this.dismiss(); }, false);
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), updateButton, cancelButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(actionGroup);

            return listComponments;
        }

        private DialogGUIHorizontalLayout createConstellationRow(short freq)
        {
            Color color = Constellation.getColor(freq);
            string name = Constellation.getName(freq);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, color, colorTexture);
            DialogGUILabel nameLabel = new DialogGUILabel(name, 150, 12);
            DialogGUILabel eachFreqLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(color), freq), 20, 12);
            DialogGUIButton removeButton = new DialogGUIButton("Drop", delegate { deleteFreqClick(freq); }, 40, 32, false);
            return new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { colorImage, nameLabel, eachFreqLabel, removeButton });
        }

        /// <summary>
        /// For the dialog to call upon new user input
        /// </summary>
        private string setFreqInput(string newFreqStr)
        {
            //do nothing
            return newFreqStr;
        }

        /// <summary>
        /// For the dialog to call upon new station-name input
        /// </summary>
        private string setNameInput(string newNameInput)
        {
            //do nothing
            return newNameInput;
        }

        /// <summary>
        /// Action to revert the station's name back to the stock name
        /// </summary>
        private void defaultNameClick()
        {
            nameInput.uiItem.GetComponent<TMP_InputField>().text = this.hostStation.displaynodeName;
        }

        /// <summary>
        /// Action to add the input frequency to the temporary list to be committed
        /// </summary>
        private void addClick()
        {
            try
            {
                try // input checks
                {
                    short newFreq = short.Parse(frequencyInput.uiItem.GetComponent<TMP_InputField>().text);

                    if (newFreq < 0)
                    {
                        throw new Exception("Frequency cannot be negative");
                    }
                    else if (this.freqListShown.Contains(newFreq))
                    {
                        throw new Exception("Ground station has this frequency already");
                    }
                    else if (!Constellation.NonLinqAny(CNCCommNetScenario.Instance.constellations, newFreq))
                    {
                        throw new Exception("Please choose an existing constellation");
                    }
                    else if (!Constellation.isFrequencyValid(newFreq))
                    {
                        throw new Exception("Frequency must be between 0 and " + short.MaxValue);
                    }

                    //ALL OK
                    this.freqListShown.Add(newFreq);
                    this.freqListShown.Sort();
                    CNCLog.Debug("Added g-station freq: {0}", newFreq);
                    refreshList(this.freqListShown);
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
        }

        /// <summary>
        /// Action to remove the particular frequency from the station's list
        /// </summary>
        private void deleteFreqClick(short frequency)
        {
            this.freqListShown.Remove(frequency);
            CNCLog.Debug("Removed g-station freq: {0}", frequency);
            refreshList(this.freqListShown);
        }

        /// <summary>
        /// Launch the color picker to change the color
        /// </summary>
        private void colorEditClick()
        {
            new ColorPickerDialog(this.constellColor, userChooseColor).launch();
        }

        /// <summary>
        /// Callback for the color picker to pass the new color to
        /// </summary>
        public void userChooseColor(Color newChosenColor)
        {
            this.constellColor = newChosenColor;
            stationColorImage.uiItem.GetComponent<RawImage>().color = this.constellColor;
        }

        /// <summary>
        /// Clear and recreate the list of constellations
        /// </summary>
        private void refreshList(List<short> freqs)
        {
            List<DialogGUIBase> rows = frequencyRowLayout.children;

            //deregister
            int size = rows.Count;
            for (int i = size - 1; i >= 0; i--)
            {
                DialogGUIBase thisChild = rows[i];
                if (!(thisChild is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                {
                    rows.RemoveAt(i);
                    thisChild.uiItem.gameObject.DestroyGameObjectImmediate();
                }
            }

            //create
            for (int i = 0; i < freqs.Count; i++)
            {
                rows.Add(createConstellationRow(freqs[i]));
            }

            //register
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(frequencyRowLayout.uiItem.gameObject.transform);
            for (int i = 0; i < rows.Count; i++)
            {
                if (!(rows[i] is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                    rows[i].Create(ref stack, HighLogic.UISkin); // recursively create child's children
            }
        }

        /// <summary>
        /// Action to update the ground station
        /// </summary>
        private void updateAction()
        {
            bool changesCommitted = false;
            string newName = nameInput.uiItem.GetComponent<TMP_InputField>().text.Trim();

            if (!this.hostStation.stationName.Equals(newName)) // different name
            {
                if(newName.Equals(this.hostStation.displaynodeName))
                    this.hostStation.stationName = "";
                else
                    this.hostStation.stationName = newName;

                ScreenMessage msg = new ScreenMessage(string.Format("Ground station is renamed to '{0}'", this.hostStation.stationName), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
                changesCommitted = true;
            }

            if(this.hostStation.Color != this.constellColor)
            {
                this.hostStation.Color = this.constellColor;
                ScreenMessage msg = new ScreenMessage(string.Format("Ground station is '{0}' now", UIUtils.colorToHex(this.hostStation.Color)), CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
                changesCommitted = true;
            }

            int commonFreq = Constellation.NonLinqIntersect(this.freqListShown, this.hostStation.Frequencies).Count;
            if (!(commonFreq == this.hostStation.Frequencies.Count && commonFreq == this.freqListShown.Count))
            {
                this.hostStation.Frequencies = this.freqListShown;
                ScreenMessage msg = new ScreenMessage("Ground station's frequency list is updated", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
                changesCommitted = true;
            }

            string strFreqs = ""; for (int i = 0; i < this.freqListShown.Count; i++) strFreqs += this.freqListShown[i]+" ";
            CNCLog.Debug("Updated g-station: {0}, {1}", newName, strFreqs);

            if (changesCommitted)
            {
                this.dismiss();
            }
        }
    }
}