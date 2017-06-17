using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using static CommNetConstellation.CommNetLayer.CNCCommNetVessel;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit the constellation membership of this vessel (Controller)
    /// </summary>
    public class VesselSetupDialog : AbstractDialog
    {
        protected enum Tool : short { SELECT_ANTENNAS, BUILD_LIST };

        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private Callback<Vessel> updateCallback;

        private DialogGUIVerticalLayout frequencyRowLayout;
        private DialogGUIVerticalLayout toolContentLayout;
        private DialogGUIVerticalLayout toggleAntennaColumn;
        private UIStyle style;

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");

        public VesselSetupDialog(string title, Vessel vessel, Callback<Vessel>  updateCallback) : base("vesselEdit",
                                                                                                                title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                500, //width
                                                                                                                500, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.updateCallback = updateCallback;
            this.description = string.Format("The frequency list of this vessel '{0}' is used to communicate with other vessels.", this.hostVessel.vesselName);

            style = new UIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Normal;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            if(this.updateCallback != null)
                this.updateCallback(this.hostVessel);

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
            DialogGUIButton buildButton = new DialogGUIButton("Update List", delegate { displayContent(Tool.BUILD_LIST); }, 50, 32, false);
            DialogGUIButton selectAntButton = new DialogGUIButton("Antennas", delegate { displayContent(Tool.SELECT_ANTENNAS); }, 40, 32, false);
            DialogGUILabel comingSoonLabel = new DialogGUILabel("More coming tools soon!");
            DialogGUIHorizontalLayout tabbedButtonRow = new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { buildButton, selectAntButton, new DialogGUISpace(3), comingSoonLabel, new DialogGUIFlexibleSpace() });
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

        private void refreshFrequencyRows()
        {
            deregisterLayoutComponents(frequencyRowLayout);

            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
            List<short> vesselFrequencyList = cncVessel.getFrequencies();
            for (int i = 0; i < vesselFrequencyList.Count; i++)
            {
                frequencyRowLayout.AddChild(createFrequencyRow(vesselFrequencyList[i]));
            }

            registerLayoutComponents(frequencyRowLayout);
        }

        private void displayContent(Tool tool)
        {
            deregisterLayoutComponents(toolContentLayout);

            DialogGUIBase[] layout;
            switch (tool)
            {
                case Tool.SELECT_ANTENNAS:
                    layout = drawTool_antennas();
                    break;
                case Tool.BUILD_LIST:
                    layout = drawTool_buildlist();
                    break;
                default:
                    layout = new DialogGUIBase[] { };
                    break;
            }
            toolContentLayout.AddChildren(layout);

            registerLayoutComponents(toolContentLayout);
        }

        private DialogGUIBase[] drawTool_buildlist()
        {
            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
            List<DialogGUIBase> layout = new List<DialogGUIBase>();
            DialogGUILabel msgLbl = new DialogGUILabel("Decide how the vessel's frequency list is updated whenever one antenna is changed (eg deployed/retracted or frequency change)\n");
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            DialogGUIToggleGroup toggleGrp = new DialogGUIToggleGroup();
            DialogGUIVerticalLayout nameColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout descriptionColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            DialogGUIToggle toggleBtn1 = new DialogGUIToggle((cncVessel.FreqListOperation == FrequencyListOperation.AutoBuild) ? true: false, "", delegate (bool b) { ListOperationSelected(b, FrequencyListOperation.AutoBuild); }, 20, 32);
            DialogGUILabel nameLabel1 = new DialogGUILabel("Auto Build", style); nameLabel1.size = new Vector2(80, 32);
            DialogGUILabel descriptionLabel1 = new DialogGUILabel("Rebuild the list from all antennas automatically", style); descriptionLabel1.size = new Vector2(350, 32);
            toggleGrp.AddChild(toggleBtn1);
            nameColumn.AddChild(nameLabel1);
            descriptionColumn.AddChild(descriptionLabel1);

            DialogGUIToggle toggleBtn2 = new DialogGUIToggle((cncVessel.FreqListOperation == FrequencyListOperation.LockList) ? true : false, "", delegate (bool b) { ListOperationSelected(b, FrequencyListOperation.LockList); }, 20, 32);
            DialogGUILabel nameLabel2 = new DialogGUILabel("Lock List", style); nameLabel2.size = new Vector2(80, 32);
            DialogGUILabel descriptionLabel2 = new DialogGUILabel("Disallow any change in the current list", style); descriptionLabel2.size = new Vector2(350, 32);
            toggleGrp.AddChild(toggleBtn2);
            nameColumn.AddChild(nameLabel2);
            descriptionColumn.AddChild(descriptionLabel2);

            DialogGUIToggle toggleBtn3 = new DialogGUIToggle((cncVessel.FreqListOperation == FrequencyListOperation.UpdateOnly) ? true : false, "", delegate (bool b) { ListOperationSelected(b, FrequencyListOperation.UpdateOnly); }, 20, 32);
            DialogGUILabel nameLabel3 = new DialogGUILabel("Update Only", style); nameLabel3.size = new Vector2(80, 32);
            DialogGUILabel descriptionLabel3 = new DialogGUILabel("Update the affected frequency in the list only (not yet)", style); descriptionLabel3.size = new Vector2(350, 32);
            toggleGrp.AddChild(toggleBtn3);
            nameColumn.AddChild(nameLabel3);
            descriptionColumn.AddChild(descriptionLabel3);

            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft, toggleGrp), nameColumn, descriptionColumn }));

            return layout.ToArray();
        }

        private void ListOperationSelected(bool b, FrequencyListOperation operation)
        {
            if(b)
            {
                CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
                cncVessel.FreqListOperation = operation;
            }
        }

        private DialogGUIBase[] drawTool_antennas()
        {
            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
            List<CNCAntennaPartInfo> allAntennas = cncVessel.getAllAntennaInfo(true);

            List<DialogGUIBase> layout = new List<DialogGUIBase>();
            DialogGUILabel msgLbl = new DialogGUILabel("Choose some antennas to build the frequency list\n");
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            toggleAntennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout nameColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout comPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout frequencyColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout combinableColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            for (int i = 0; i < allAntennas.Count(); i++)
            {
                CNCAntennaPartInfo antennaInfo = allAntennas[i];

                DialogGUIToggle toggleBtn = new DialogGUIToggle(antennaInfo.inUse, "", delegate (bool b) { vesselAntennaSelected(b, antennaInfo.GUID); refreshFrequencyRows(); }, 20, 32);
                DialogGUILabel nameLabel = new DialogGUILabel(antennaInfo.name, style); nameLabel.size = new Vector2(160, 32);
                DialogGUILabel comPowerLabel = new DialogGUILabel(string.Format("Com power: {0:0.00}", UIUtils.RoundToNearestMetricFactor(antennaInfo.antennaPower)), style); comPowerLabel.size = new Vector2(120, 32);
                DialogGUILabel frequencyLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(Constellation.getColor(antennaInfo.frequency)), antennaInfo.frequency), style); frequencyLabel.size = new Vector2(60, 32);
                DialogGUILabel combinableLabel = new DialogGUILabel("Combinable: "+(antennaInfo.antennaCombinable?"Yes":"No")+"\nBroadcast: "+(antennaInfo.canComm?"Yes":"No"), style); combinableLabel.size = new Vector2(90, 32);

                toggleAntennaColumn.AddChild(toggleBtn);
                nameColumn.AddChild(nameLabel);
                frequencyColumn.AddChild(frequencyLabel);
                comPowerColumn.AddChild(comPowerLabel);
                combinableColumn.AddChild(combinableLabel);
            }

            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleAntennaColumn, nameColumn, frequencyColumn, comPowerColumn, combinableColumn}));

            DialogGUIButton deselectButton = new DialogGUIButton("Deselect all", delegate { toggleAllAntennas(false); refreshFrequencyRows(); }, false);
            DialogGUIButton selectButton = new DialogGUIButton("Select all", delegate { toggleAllAntennas(true); refreshFrequencyRows(); }, false);
            //DialogGUIButton buildButton = new DialogGUIButton("Build List", delegate { cncVessel.rebuildFreqList(); refreshFrequencyRows(); }, false);
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { selectButton, deselectButton }));

            return layout.ToArray();
        }

        private void vesselAntennaSelected(bool useState, uint antennaGUID)
        {
            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
            cncVessel.toggleAntenna(antennaGUID, useState);
            cncVessel.OnAntennaChange();            
        }

        private void toggleAllAntennas(bool state)
        {
            CNCCommNetVessel cncVessel = (CNCCommNetVessel)this.hostVessel.Connection;
            List<CNCAntennaPartInfo> allAntennas = cncVessel.getAllAntennaInfo();

            for (int i = 0; i < allAntennas.Count; i++)
                vesselAntennaSelected(state, allAntennas[i].GUID);

            displayContent(Tool.SELECT_ANTENNAS);
        }
    }
}
