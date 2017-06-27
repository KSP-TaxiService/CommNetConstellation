using System;
using System.Collections.Generic;
using CommNet;
using UnityEngine;
using CommNetConstellation.CommNetLayer;

namespace CommNetConstellation.UI.VesselMgtTools
{
    public class AntennaTool : AbstractMgtTool
    {
        private DialogGUIVerticalLayout toggleAntennaColumn;

        public AntennaTool(CommNetVessel thisVessel, Callback updateFreqRowsCallback) : base(thisVessel, "antenna", "Antennas", updateFreqRowsCallback)
        {
        }

        public override List<DialogGUIBase> getContentComponents()
        {
            List<DialogGUIBase> layout = new List<DialogGUIBase>();

            DialogGUILabel msgLbl = new DialogGUILabel("Choose some antennas to build the frequency list\n");
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            toggleAntennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout nameColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout comPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout frequencyColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout combinableColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            for (int i = 0; i < antennas.Count; i++)
            {
                CNCAntennaPartInfo antennaInfo = antennas[i];

                DialogGUIToggle toggleBtn = new DialogGUIToggle(antennaInfo.inUse, "", delegate (bool b) { vesselAntennaSelected(b, antennaInfo.GUID); updateCallback(); }, 20, 32);
                DialogGUILabel nameLabel = new DialogGUILabel(antennaInfo.name, style); nameLabel.size = new Vector2(160, 32);
                DialogGUILabel comPowerLabel = new DialogGUILabel(string.Format("Com power: {0:0.00}", UIUtils.RoundToNearestMetricFactor(antennaInfo.antennaPower)), style); comPowerLabel.size = new Vector2(120, 32);
                DialogGUILabel frequencyLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(Constellation.getColor(antennaInfo.frequency)), antennaInfo.frequency), style); frequencyLabel.size = new Vector2(60, 32);
                DialogGUILabel combinableLabel = new DialogGUILabel("Combinable: " + (antennaInfo.antennaCombinable ? "Yes" : "No") + "\nBroadcast: " + (antennaInfo.canComm ? "Yes" : "No"), style); combinableLabel.size = new Vector2(90, 32);

                toggleAntennaColumn.AddChild(toggleBtn);
                nameColumn.AddChild(nameLabel);
                frequencyColumn.AddChild(frequencyLabel);
                comPowerColumn.AddChild(comPowerLabel);
                combinableColumn.AddChild(combinableLabel);
            }

            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleAntennaColumn, nameColumn, frequencyColumn, comPowerColumn, combinableColumn }));

            DialogGUIButton deselectButton = new DialogGUIButton("Deselect all", delegate { toggleAllAntennas(false); updateCallback(); }, false);
            DialogGUIButton selectButton = new DialogGUIButton("Select all", delegate { toggleAllAntennas(true); updateCallback(); }, false);
            //DialogGUIButton buildButton = new DialogGUIButton("Build List", delegate { cncVessel.rebuildFreqList(); refreshFrequencyRows(); }, false);
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { selectButton, deselectButton }));

            return layout;
        }

        public override void run()
        {
            throw new NotImplementedException();
        }

        private void vesselAntennaSelected(bool useState, uint antennaGUID)
        {
            cncVessel.toggleAntenna(antennaGUID, useState);
            cncVessel.OnAntennaChange();
        }

        private void toggleAllAntennas(bool state)
        {
            List<CNCAntennaPartInfo> allAntennas = cncVessel.getAllAntennaInfo();

            for (int i = 0; i < allAntennas.Count; i++)
                vesselAntennaSelected(state, allAntennas[i].GUID);

            //displayContent(Tool.SELECT_ANTENNAS);
        }
    }
}
