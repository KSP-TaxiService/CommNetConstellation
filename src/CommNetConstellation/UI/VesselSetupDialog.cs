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
        protected enum ToolNames : short { DEACT_ANTENNAS };

        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private Callback<Vessel, short> updateCallback;

        private DialogGUIVerticalLayout frequencyRowLayout;
        private DialogGUIVerticalLayout toolContentLayout;
        private UIStyle style;

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel vessel, Callback<Vessel, short>  updateCallback) : base("vesselEdit",
                                                                                                                title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                500, //width
                                                                                                                500, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.updateCallback = updateCallback;
            this.description = string.Format("The frequency list of this vessel '{0}' is used to communicate with other vessels of the same frequencies.", this.hostVessel.vesselName);

            style = new UIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Normal;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;

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
            listComponments.Add(new DialogGUILabel("\n<b>Active frequencies</b>", false, false));
            DialogGUIBase[] frequencyRows = new DialogGUIBase[vesselFrequencyList.Count + 1];
            frequencyRows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < vesselFrequencyList.Count; i++)
            {
                frequencyRows[i + 1] = createFrequencyRow(vesselFrequencyList[i]);
            }

            frequencyRowLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, frequencyRows);
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, frequencyRowLayout));

            //tools
            listComponments.Add(new DialogGUILabel("\n<b>Management tools</b>", false, false));
            //Button tabs
            DialogGUIButton deactButton = new DialogGUIButton("Antennas", delegate { displayContent(ToolNames.DEACT_ANTENNAS); }, 40, 32, false);
            DialogGUILabel comingSoonLabel = new DialogGUILabel("More coming tools soon!");
            DialogGUIHorizontalLayout tabbedButtonRow = new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { deactButton, new DialogGUISpace(3), comingSoonLabel, new DialogGUIFlexibleSpace() });
            listComponments.Add(tabbedButtonRow);

            //Tool content
            toolContentLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[]{ new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) });
            listComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, toolContentLayout));

            return listComponments;
        }

        private DialogGUIHorizontalLayout createFrequencyRow(short freq)
        {
            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
            Color color = Constellation.getColor(freq);
            string name = Constellation.getName(freq);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, color, colorTexture);
            DialogGUILabel nameLabel = new DialogGUILabel(name, 150, 12);
            DialogGUILabel eachFreqLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(color), freq), 20, 12);
            DialogGUILabel freqPowerLabel = new DialogGUILabel(string.Format("Combined Comm Power: {0}", UIUtils.RoundToNearestMetricFactor(cncVessel.getMaxComPower(freq))), 150, 12);
            return new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { colorImage, nameLabel, eachFreqLabel, freqPowerLabel });
        }

        private void displayContent(ToolNames newType)
        {
            deregisterLayoutComponents(toolContentLayout);
            toolContentLayout.AddChildren(drawTool_antennas());
            registerLayoutComponents(toolContentLayout);
        }

        private DialogGUIBase[] drawTool_antennas()
        {
            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;

            List<DialogGUIBase> layout = new List<DialogGUIBase>();
            List<CNCAntennaPartInfo> antennas = cncVessel.getAntennaInfo();

            DialogGUIVerticalLayout antennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout comPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout frequencyColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout combinableColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            for (int i = 0; i < antennas.Count(); i++)
            {
                CNCAntennaPartInfo antennaInfo = antennas[i];
                int antennaIndex = i; // antennaModules.Count doesn't work due to the compiler optimization

                DialogGUIToggle toggleBtn = new DialogGUIToggle(false, antennaInfo.name, delegate (bool b) { vesselAntennaSelected(b, antennaIndex); }, 170, 32);
                DialogGUILabel comPowerLabel = new DialogGUILabel(string.Format("Com power: {0:0.00}", UIUtils.RoundToNearestMetricFactor(antennaInfo.antennaPower)), style); comPowerLabel.size = new Vector2(120, 32);
                DialogGUILabel frequencyLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(Constellation.getColor(antennaInfo.frequency)), antennaInfo.frequency), style); frequencyLabel.size = new Vector2(60, 32);
                DialogGUILabel combinableLabel = new DialogGUILabel("Combinable: "+(antennaInfo.antennaCombinable?"Yes":"No"), style); combinableLabel.size = new Vector2(90, 32);

                antennaColumn.AddChild(toggleBtn);
                frequencyColumn.AddChild(frequencyLabel);
                comPowerColumn.AddChild(comPowerLabel);
                combinableColumn.AddChild(combinableLabel);
            }

            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { antennaColumn, frequencyColumn, comPowerColumn, combinableColumn}));

            DialogGUIButton deselectButton = new DialogGUIButton("Select all", delegate { }, false);
            DialogGUIButton selectButton = new DialogGUIButton("Deselect all", delegate { }, false);
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { selectButton, deselectButton }));

            return layout.ToArray();
        }

        private void vesselAntennaSelected(bool b, int antennaIndex)
        {
            
        }

    }
}
