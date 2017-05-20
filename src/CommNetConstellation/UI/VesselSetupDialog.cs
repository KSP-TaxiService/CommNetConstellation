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
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private Callback<Vessel, short> updateCallback;

        private DialogGUIVerticalLayout frequencyRowLayout;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel vessel, Callback<Vessel, short>  updateCallback) : base(title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                500, //width
                                                                                                                400, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.updateCallback = updateCallback;
            this.description = string.Format("The frequency list of this vessel '{0}' is used to communicate with other vessels of the same frequencies.", this.hostVessel.vesselName);

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
            List<short> vesselFrequencyList = cncVessel.getFrequencies();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description + "\n", false, false) }));

            //frequency list
            listComponments.Add(new DialogGUILabel("\n<b>Current frequency list</b>", false, false));
            DialogGUIBase[] frequencyRows = new DialogGUIBase[vesselFrequencyList.Count + 1];
            frequencyRows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < vesselFrequencyList.Count; i++)
            {
                frequencyRows[i + 1] = createFrequencyRow(vesselFrequencyList[i]);
            }

            frequencyRowLayout = new DialogGUIVerticalLayout(10, 100, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, frequencyRows);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, frequencyRowLayout));

            //selectable antennas
            listComponments.Add(new DialogGUILabel("\n<b>Antennas</b>", false, false));

            DialogGUIVerticalLayout antennaLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { });
            antennaLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUIVerticalLayout antennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout comPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout frequencyColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            for (int i = 0; i < cncVessel.getNumberAntennas(); i++)
            {
                CNCAntennaPartInfo antennaInfo = cncVessel.getAntennaInfo(i);
                int antennaIndex = i; // antennaModules.Count doesn't work due to the compiler optimization

                DialogGUIToggle toggleBtn = new DialogGUIToggle(false, antennaInfo.name, delegate (bool b) { vesselAntennaSelected(b, antennaIndex); }, 170, 32);
                DialogGUILabel comPowerLabel = new DialogGUILabel(string.Format("Com power: {0:0.00}", UIUtils.RoundToNearestMetricFactor(antennaInfo.antennaPower))); comPowerLabel.size = new Vector2(150, 32);
                DialogGUILabel frequencyLabel = new DialogGUILabel(string.Format("Frequency: <color={0}>{1}</color>", UIUtils.colorToHex(Constellation.getColor(antennaInfo.frequency)), antennaInfo.frequency)); frequencyLabel.size = new Vector2(150, 32);

                antennaColumn.AddChild(toggleBtn);
                frequencyColumn.AddChild(frequencyLabel);
                comPowerColumn.AddChild(comPowerLabel);
            }

            antennaLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { antennaColumn, frequencyColumn, comPowerColumn }));
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, antennaLayout));


            //tools
            listComponments.Add(new DialogGUILabel("\n<b>Management tools</b>", false, false));

            return listComponments;
        }

        private DialogGUIHorizontalLayout createFrequencyRow(short freq)
        {
            Color color = Constellation.getColor(freq);
            string name = Constellation.getName(freq);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, color, colorTexture);
            DialogGUILabel nameLabel = new DialogGUILabel(name, 150, 12);
            DialogGUILabel eachFreqLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(color), freq), 20, 12);
            DialogGUILabel freqPowerLabel = new DialogGUILabel(string.Format("Combined Comm Power: {0}", UIUtils.RoundToNearestMetricFactor(1234)), 80, 12);//TODO: do the comm power combination
            return new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { colorImage, nameLabel, eachFreqLabel, freqPowerLabel });
        }

        private void vesselAntennaSelected(bool b, int antennaIndex)
        {
            
        }

    }
}
