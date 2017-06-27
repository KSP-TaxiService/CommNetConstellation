using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using static CommNetConstellation.CommNetLayer.CNCCommNetVessel;
using CommNetConstellation.UI.VesselMgtTools;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit the constellation membership of this vessel (Controller)
    /// </summary>
    public class VesselSetupDialog : AbstractDialog
    {
        private Vessel hostVessel; // could be null (in editor)
        private string description = "Something";

        private Callback<Vessel> updateCallback;

        private DialogGUIVerticalLayout frequencyRowLayout;
        private DialogGUIVerticalLayout toolContentLayout;
        private List<AbstractMgtTool> tools;

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
            this.tools = new List<AbstractMgtTool>();

            UpdateListTool updateTool = new UpdateListTool(this.hostVessel.connection);
            this.tools.Add(updateTool);
            AntennaTool antennaTool = new AntennaTool(this.hostVessel.connection, refreshFrequencyRows);
            this.tools.Add(antennaTool);
            VanillaFreqTool vanillaTool = new VanillaFreqTool(this.hostVessel.connection);
            this.tools.Add(vanillaTool);

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
            DialogGUIBase[] buttons = new DialogGUIBase[this.tools.Count+1];
            for (int i=0; i<this.tools.Count; i++)
            {
                AbstractMgtTool thisTool = this.tools[i];
                buttons[i] = new DialogGUIButton(thisTool.toolName, delegate { displayContent(thisTool.getContentComponents()); }, 50, 32, false);
            }
            buttons[this.tools.Count] = new DialogGUILabel("More coming tools soon!");            
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, buttons));

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

        private void displayContent(List<DialogGUIBase> contents)
        {
            deregisterLayoutComponents(toolContentLayout);
            toolContentLayout.AddChildren(contents.ToArray());
            registerLayoutComponents(toolContentLayout);
        }
    }
}
