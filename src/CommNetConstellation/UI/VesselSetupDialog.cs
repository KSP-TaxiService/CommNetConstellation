using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommNetConstellation.UI.VesselMgtTools;
using CommNetConstellation.UI.DialogGUI;
using KSP.Localization;
using CommNetManagerAPI;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Edit the constellation membership of this vessel (Controller)
    /// </summary>
    public class VesselSetupDialog : AbstractDialog
    {
        private Vessel hostVessel; // could be null (in editor)
        private CNCCommNetVessel cncVessel = null;
        private string description = Localizer.Format("#CNC_AntennaSetup_DescText1");// "Something"

        private const string nofreqMessage = "#CNC_VesselSetup_nofreqMessage";//"No active frequency to broadcast!"
        private UIStyle nofreqMessageStyle;

        private Callback<Vessel> updateCallback;
        private DialogGUIVerticalLayout frequencyRowLayout;
        private ToolContentManagement toolMgt;
        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private CustomDialogGUIScrollList scrollArea;

        public VesselSetupDialog(string title, Vessel vessel, Callback<Vessel>  updateCallback) : base("vesselEdit",
                                                                                                                title, 
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                550, //width
                                                                                                                600, //height
                                                                                                                new DialogOptions[] {})
        {
            this.hostVessel = vessel;
            this.cncVessel = ((IModularCommNetVessel)hostVessel.Connection).GetModuleOfType<CNCCommNetVessel>();
            this.updateCallback = updateCallback;
            this.description = Localizer.Format("#CNC_VesselSetup_desc", this.hostVessel.GetDisplayName());//string.Format("Active frequencies allow this vessel '{0}' to talk with other vessels, which share one or more of these frequencies.", )

            this.toolMgt = new ToolContentManagement(500, 100);
            UpdateListTool updateTool = new UpdateListTool(this.hostVessel.connection);
            this.toolMgt.add(updateTool);
            AntennaSelectionTool antennaTool = new AntennaSelectionTool(this.hostVessel.connection, refreshFrequencyRows);
            this.toolMgt.add(antennaTool);
            IndepAntennaFreqTool antennaTool2 = new IndepAntennaFreqTool(this.hostVessel.connection, refreshFrequencyRows);
            this.toolMgt.add(antennaTool2);
            VanillaFreqTool vanillaTool = new VanillaFreqTool(this.hostVessel.connection, refreshFrequencyRows);
            this.toolMgt.add(vanillaTool);

            this.nofreqMessageStyle = new UIStyle();
            this.nofreqMessageStyle.alignment = TextAnchor.MiddleCenter;
            this.nofreqMessageStyle.fontStyle = FontStyle.Bold;
            this.nofreqMessageStyle.normal = HighLogic.UISkin.label.normal;

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            this.updateCallback?.Invoke(this.hostVessel);
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            List<short> vesselFrequencyList = cncVessel.getFrequencyList();
            vesselFrequencyList.Sort();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description + "\n\n", false, false) }));

            //frequency list
            listComponments.Add(new DialogGUILabel("<b>" + Localizer.Format("#CNC_VesselSetup_ActiveFrequencies") + "</b>", false, false));//Active frequencies
            DialogGUIBase[] frequencyRows;
            if (vesselFrequencyList.Count == 0)
            {
                frequencyRows = new DialogGUIBase[2];
                frequencyRows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
                frequencyRows[1] = new DialogGUILabel(Localizer.Format(nofreqMessage), nofreqMessageStyle, true, false);
            }
            else
            {
                frequencyRows = new DialogGUIBase[vesselFrequencyList.Count + 1];
                frequencyRows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
                for (int i = 0; i < vesselFrequencyList.Count; i++)
                {
                    frequencyRows[i + 1] = createFrequencyRow(vesselFrequencyList[i]);
                }
            }
            frequencyRowLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, frequencyRows);
            scrollArea = new CustomDialogGUIScrollList(new Vector2(450, 100), false, true, frequencyRowLayout);
            listComponments.Add(scrollArea);

            //tools
            listComponments.AddRange(this.toolMgt.getLayoutContents());

            return listComponments;
        }

        private DialogGUIHorizontalLayout createFrequencyRow(short freq)
        {
            Color color = Constellation.getColor(freq);
            string name = Constellation.getName(freq);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, color, colorTexture);
            DialogGUILabel nameLabel = new DialogGUILabel(name, 170, 12);
            DialogGUILabel eachFreqLabel = new DialogGUILabel(string.Format("(<color={0}>{1}</color>)", UIUtils.colorToHex(color), freq), 70, 12);
            DialogGUILabel freqPowerLabel = new DialogGUILabel( Localizer.Format("#CNC_VesselSetup_CombinedCommPower") + string.Format(": {0}", UIUtils.RoundToNearestMetricFactor(cncVessel.getMaxComPower(freq), 2)), 220, 12);//Combined Comm Power
            return new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { colorImage, new DialogGUISpace(20), nameLabel, eachFreqLabel, freqPowerLabel });
        }

        private void refreshFrequencyRows()
        {
            deregisterLayoutComponents(frequencyRowLayout);

            List<short> vesselFrequencyList = cncVessel.getFrequencyList();
            vesselFrequencyList.Sort();

            for (int i = 0; i < vesselFrequencyList.Count; i++)
            {
                frequencyRowLayout.AddChild(createFrequencyRow(vesselFrequencyList[i]));
            }

            if (vesselFrequencyList.Count == 0)
            {
                frequencyRowLayout.AddChild(new DialogGUILabel(Localizer.Format(nofreqMessage), nofreqMessageStyle, true, false));
            }

            registerLayoutComponents(frequencyRowLayout);
            scrollArea.Resize();
        }
    }
}
