using CommNetConstellation.CommNetLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static MapViewFiltering;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Interact with constellations or vessels (Controller)
    /// </summary>
    public class ConstellationControlDialog : AbstractDialog
    {
        public enum VesselListSort {LAUNCHDATE, RADIOFREQ, VESSELNAME, CBODY };

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private static readonly Texture2D focusTexture = UIUtils.loadImage("focusEye");

        private DialogGUIVerticalLayout constellationRowLayout;
        private DialogGUIVerticalLayout vesselRowLayout;
        private DialogGUIScrollList vesselScrollPane;
        private VesselListSort currentVesselSort;

        public ConstellationControlDialog(string title) : base("CNCControl",
                                                            title, 
                                                            0.8f, //x
                                                            0.5f, //y
                                                            (int)(1920*0.3), //width
                                                            (int)(1200*0.5), //height
                                                            new DialogOptions[] { DialogOptions.ShowVersion, DialogOptions.HideCloseButton, DialogOptions.AllowBgInputs }) //arguments
        {
            
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();
            listComponments.AddRange(setupConstellationList());
            listComponments.AddRange(setupVesselList());

            return listComponments;
        }

        protected override void OnAwake(object[] args)
        {
            GameEvents.OnMapViewFiltersModified.Add(new EventData<MapViewFiltering.VesselTypeFilter>.OnEvent(this.mapfilterChanged));
        }

        protected override void OnPreDismiss()
        {
            GameEvents.OnMapViewFiltersModified.Remove(new EventData<MapViewFiltering.VesselTypeFilter>.OnEvent(this.mapfilterChanged));
        }

        /////////////////////
        // CONSTELLATIONS
        /////////////////////

        /////////////////////
        // GUI
        private List<DialogGUIBase> setupConstellationList()
        {
            List<DialogGUIBase> constellationComponments = new List<DialogGUIBase>();
            //constellationComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel("\n<b>You can manage multiple constellations of vessels.</b>", false, false) }));
            constellationComponments.Add(new DialogGUILabel("\n<b>You can manage multiple constellations of vessels.</b>", false, false));

            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();

            DialogGUIButton createButton = new DialogGUIButton("New constellation", newConstellationClick, false);
            DialogGUIHorizontalLayout creationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), createButton, new DialogGUIFlexibleSpace() });
            eachRowGroupList.Add(creationGroup);

            for (int i = 0; i < CNCCommNetScenario.Instance.constellations.Count; i++)
                eachRowGroupList.Add(createConstellationRow(CNCCommNetScenario.Instance.constellations[i]));

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            constellationRowLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperCenter, rows);
            constellationComponments.Add(new DialogGUIScrollList(Vector2.one, false, true, constellationRowLayout));

            return constellationComponments;
        }

        private DialogGUIHorizontalLayout createConstellationRow(Constellation thisConstellation)
        {
            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, thisConstellation.color, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(thisConstellation.name, 150, 12);
            DialogGUILabel freqLabel = new DialogGUILabel(string.Format("Frequency: {0}", thisConstellation.frequency), 110, 12);
            DialogGUILabel numSatsLabel = new DialogGUILabel(string.Format("{0} vessels", Constellation.countVessels(thisConstellation)), 80, 12);
            DialogGUIButton updateButton = new DialogGUIButton("Edit", delegate { editConstellationClick(thisConstellation); }, 50, 32, false);

            DialogGUIBase[] rowGUIBase = new DialogGUIBase[] { colorImage, constNameLabel, freqLabel, numSatsLabel, updateButton, null };
            if (thisConstellation.frequency == CNCSettings.Instance.PublicRadioFrequency)
                rowGUIBase[rowGUIBase.Length - 1] = new DialogGUIButton("Reset", resetPublicConstClick, 60, 32, false);
            else
                rowGUIBase[rowGUIBase.Length - 1] = new DialogGUIButton("Delete", delegate { deleteConstellationClick(thisConstellation); }, 60, 32, false);

            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, rowGUIBase);
            constellationGroup.SetOptionText(thisConstellation.frequency.ToString()); //for quick identification
            return constellationGroup;
        }

        private int deleteConstellationGUIRow(Constellation thisConstellation)
        {
            List<DialogGUIBase> rows = constellationRowLayout.children;

            for (int i = 2; i < rows.Count; i++)
            {
                DialogGUIBase thisRow = rows[i];
                if (thisRow.OptionText.Equals(thisConstellation.frequency.ToString()))
                {
                    rows.RemoveAt(i); // drop from the scrolllist rows
                    for (int j = thisRow.children.Count - 1; j >= 0; j--)// necessary to free memory up
                        thisRow.children[j].uiItem.gameObject.DestroyGameObjectImmediate();
                    thisRow.uiItem.gameObject.DestroyGameObjectImmediate();
                    return i;
                }
            }

            return -1;
        }

        private void updateConstellationGUIRow(short updatedfrequency, short previousFrequency)
        {
            List<DialogGUIBase> rows = constellationRowLayout.children;

            for (int i = 2; i < rows.Count; i++)
            {
                DialogGUIBase thisRow = rows[i];
                if (thisRow.OptionText.Equals(updatedfrequency.ToString()) || thisRow.OptionText.Equals(previousFrequency.ToString()))
                {
                    DialogGUIImage colorImage = thisRow.children[0] as DialogGUIImage;
                    DialogGUILabel nameLabel = thisRow.children[1] as DialogGUILabel;
                    DialogGUILabel freqLabel = thisRow.children[2] as DialogGUILabel;
                    DialogGUILabel vesselLabel = thisRow.children[3] as DialogGUILabel;

                    Constellation updatedConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == updatedfrequency);
                    colorImage.uiItem.GetComponent<RawImage>().color = updatedConstellation.color;
                    nameLabel.SetOptionText(updatedConstellation.name);
                    freqLabel.SetOptionText(string.Format("Frequency: <color={0}>{1}</color>", UIUtils.colorToHex(updatedConstellation.color), updatedConstellation.frequency));
                    vesselLabel.SetOptionText(Constellation.countVessels(updatedConstellation) + " vessels");

                    thisRow.SetOptionText(updatedConstellation.frequency.ToString());
                    break;
                }
            }
        }

        private void resetPublicConstClick()
        {
            string message = string.Format("Revert to the default name '{0}' and color {1}?", CNCSettings.Instance.DefaultPublicName, UIUtils.colorToHex(CNCSettings.Instance.DefaultPublicColor));
            MultiOptionDialog warningDialog = new MultiOptionDialog("cncResetConstWindow", message, "Constellation", HighLogic.UISkin, new DialogGUIBase[]
            {
                new DialogGUIButton("Reset", resetPublicConstellation, true),
                new DialogGUIButton("Cancel", delegate { }, true)
            });

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), warningDialog, false, HighLogic.UISkin, true, string.Empty);
        }

        private void deleteConstellationClick(Constellation thisConstellation)
        {
            string title = string.Format("Deleting '{0}'?", thisConstellation.name);
            string message = string.Format("All the vessels of Constellation '{0}' will be reintegrated into the public constellation.", thisConstellation.name);

            MultiOptionDialog warningDialog = new MultiOptionDialog("cncDeleteConstWindow", message, title, HighLogic.UISkin, new DialogGUIBase[]
            {
                new DialogGUIButton("Delete", delegate { deleteConstellation(thisConstellation); }, true),
                new DialogGUIButton("Cancel", delegate { }, true)
            });

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), warningDialog, false, HighLogic.UISkin, true);
        }

        private void newConstellationClick()
        {
            new ConstellationEditDialog("Constellation - <color=#00ff00>New</color>", null, createNewConstellation, null).launch();
        }

        private void editConstellationClick(Constellation thisConstellation)
        {
            new ConstellationEditDialog("Constellation - <color=#00ff00>Edit</color>", thisConstellation, null, updateConstellation).launch();
        }

        /////////////////////
        // Actions
        /// <summary>
        /// Action to reset the public constellation
        /// </summary>
        private void resetPublicConstellation()
        {
            Constellation publicConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == CNCSettings.Instance.PublicRadioFrequency);
            publicConstellation.name = CNCSettings.Instance.DefaultPublicName;
            publicConstellation.color = CNCSettings.Instance.DefaultPublicColor;
            updateConstellation(publicConstellation, CNCSettings.Instance.PublicRadioFrequency);
        }

        /// <summary>
        /// Action to remove the constellation from the record and save
        /// </summary>
        private void deleteConstellation(Constellation deletedConstellation)
        {
            if (deleteConstellationGUIRow(deletedConstellation) >= 0)
            {
                CNCCommNetScenario.Instance.constellations.RemoveAt(CNCCommNetScenario.Instance.constellations.FindIndex(x => x.frequency == deletedConstellation.frequency));

                if (Constellation.countVessels(deletedConstellation) < 1) // no vessel to update
                    return;

                short publicFrequency = CNCSettings.Instance.PublicRadioFrequency;
                List<CNCCommNetVessel> affectedVessels = CNCCommNetScenario.Instance.getCommNetVessels().FindAll(x => x.getRadioFrequency() == deletedConstellation.frequency);
                for (int i = 0; i < affectedVessels.Count; i++)
                {
                    affectedVessels[i].updateRadioFrequency(publicFrequency);
                    updateVesselGUIRow(affectedVessels[i].Vessel);
                }

                updateConstellationGUIRow(publicFrequency, -1);
            }
        }

        /// <summary>
        /// Action to create a new constellation and save it
        /// </summary>
        private void createNewConstellation(Constellation newConstellation)
        {
            DialogGUIHorizontalLayout newConstellationGUIRow = createConstellationRow(newConstellation);
            constellationRowLayout.AddChild(newConstellationGUIRow);

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(constellationRowLayout.uiItem.gameObject.transform); // transform effect: new row goes to the end of the list 
            newConstellationGUIRow.Create(ref stack, HighLogic.UISkin);
        }

        /// <summary>
        /// Action to change the existing constellation
        /// </summary>
        private void updateConstellation(Constellation updatedConstellation, short previousFrequency)
        {
            List<CNCCommNetVessel> affectedVessels = CNCCommNetScenario.Instance.getCommNetVessels().FindAll(x => x.getRadioFrequency() == updatedConstellation.frequency);
            for (int i = 0; i < affectedVessels.Count; i++)
                updateVesselGUIRow(affectedVessels[i].Vessel);

            updateConstellationGUIRow(updatedConstellation.frequency, previousFrequency);
        }

        /////////////////////
        // VESSELS
        /////////////////////

        /////////////////////
        // GUI
        private List<DialogGUIBase> setupVesselList()
        {
            currentVesselSort = VesselListSort.LAUNCHDATE;

            List<DialogGUIBase> vesselComponments = new List<DialogGUIBase>();
            //vesselComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel("\n<b>You can edit the constellation configuration of a vessel.</b>", false, false) }));
            vesselComponments.Add(new DialogGUILabel("\n<b>You can edit the constellation configuration of a vessel.</b>", false, false));

            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();
            eachRowGroupList.AddRange(populateVesselRows(MapViewFiltering.vesselTypeFilter));

            //Prepare a list container for the GUILayout rows
            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            vesselRowLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, rows);
            vesselScrollPane = new DialogGUIScrollList(Vector2.one, false, true, vesselRowLayout);
            vesselComponments.Add(vesselScrollPane);

            DialogGUILabel sortLabel = new DialogGUILabel("Sort by");
            DialogGUIButton launchSortBtn = new DialogGUIButton("Launch time", delegate { currentVesselSort = VesselListSort.LAUNCHDATE; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, false);
            DialogGUIButton freqSortBtn = new DialogGUIButton("Radio frequency", delegate { currentVesselSort = VesselListSort.RADIOFREQ; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, false);
            DialogGUIButton nameSortBtn = new DialogGUIButton("Vessel name", delegate { currentVesselSort = VesselListSort.VESSELNAME; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, false);
            DialogGUIButton bodySortBtn = new DialogGUIButton("Celestial body", delegate { currentVesselSort = VesselListSort.CBODY; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, false);
            vesselComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { sortLabel, launchSortBtn, freqSortBtn, nameSortBtn, bodySortBtn }));

            return vesselComponments;
        }

        private DialogGUIHorizontalLayout createVesselRow(CNCCommNetVessel thisVessel)
        {
            short radioFreq = thisVessel.getRadioFrequency();
            Color color = Constellation.getColor(radioFreq);

            UIStyle focusStyle = UIUtils.createImageButtonStyle(focusTexture);
            DialogGUIButton focusButton = new DialogGUIButton("", delegate { vesselFocusClick(thisVessel.Vessel); }, null, 32, 32, false, focusStyle);
            focusButton.image = focusStyle.normal.background;

            DialogGUILabel vesselLabel = new DialogGUILabel(thisVessel.Vessel.vesselName, 150, 12);
            DialogGUILabel freqLabel = new DialogGUILabel(string.Format("Frequency: <color={0}>{1}</color>", UIUtils.colorToHex(color), radioFreq), 110, 12);
            DialogGUILabel locationLabel = new DialogGUILabel(string.Format("Orbiting: {0}", thisVessel.Vessel.mainBody.name), 140, 12);
            DialogGUIButton setupButton = new DialogGUIButton("Setup", delegate { vesselSetupClick(thisVessel.Vessel); }, 70, 32, false);

            DialogGUIHorizontalLayout vesselGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { focusButton, vesselLabel, freqLabel, locationLabel, setupButton });
            vesselGroup.SetOptionText(thisVessel.Vessel.id.ToString());
            return vesselGroup;
        }

        private void updateVesselGUIRow(Vessel updatedVessel)
        {
            CNCCommNetVessel thisVessel = (CNCCommNetVessel)updatedVessel.Connection;
            List<DialogGUIBase> rows = vesselRowLayout.children;

            for (int i = 0; i < rows.Count; i++)
            {
                DialogGUIBase thisRow = rows[i];
                if (thisRow.OptionText.Equals(updatedVessel.id.ToString()))
                {
                    Constellation thisConstellation = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == thisVessel.getRadioFrequency());

                    DialogGUILabel freqLabel = thisRow.children[2] as DialogGUILabel;
                    freqLabel.SetOptionText(string.Format("Frequency: <color={0}>{1}</color>", UIUtils.colorToHex(thisConstellation.color), thisConstellation.frequency));
                    return;
                }
            }
        }

        private void vesselSetupClick(Vessel thisVessel)
        {
            new VesselSetupDialog("Vessel - <color=#00ff00>Setup</color>", thisVessel, null, updateVessel).launch();
        }

        private void vesselFocusClick(Vessel thisVessel)
        {
            PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.AddTarget(thisVessel.mapObject));
            PlanetariumCamera.fetch.targets.Remove(thisVessel.mapObject);
        }

        private void mapfilterChanged(MapViewFiltering.VesselTypeFilter filter)
        {
            //clear vessel rows
            List<DialogGUIBase> rows = vesselRowLayout.children;
            for (int i = rows.Count-1; i >= 1 ; i--)
            {
                DialogGUIBase thisRow = rows[i];
                rows.RemoveAt(i);
                thisRow.uiItem.gameObject.DestroyGameObjectImmediate(); // necessary to free memory up
            }
            
            List<DialogGUIHorizontalLayout> newRows = populateVesselRows(filter);
            Stack<Transform> stack = new Stack<Transform>(); // some data on hierarchy of GUI components
            stack.Push(vesselRowLayout.uiItem.gameObject.transform); // need the reference point of the parent GUI component for position and size
            for (int i = 0; i < newRows.Count; i++)
            {
                rows.Add(newRows[i]);
                rows.Last().Create(ref stack, HighLogic.UISkin); // required to force the GUI creation
            }
        }

        private List<DialogGUIHorizontalLayout> populateVesselRows(VesselTypeFilter filter)
        {
            List<DialogGUIHorizontalLayout> newRows = new List<DialogGUIHorizontalLayout>();
            List<CNCCommNetVessel> allVessels = CNCCommNetScenario.Instance.getCommNetVessels();
            IOrderedEnumerable<CNCCommNetVessel> sortedVessels;

            switch (currentVesselSort)
            {
                case VesselListSort.RADIOFREQ:
                    sortedVessels = allVessels.OrderBy(x => x.getRadioFrequency());
                    break;
                case VesselListSort.VESSELNAME:
                    sortedVessels = allVessels.OrderBy(x => x.Vessel.GetName());
                    break;
                case VesselListSort.CBODY:
                    sortedVessels = allVessels.OrderBy(x => x.Vessel.mainBody.name);
                    break;
                default:
                    sortedVessels = allVessels.OrderBy(x => x.Vessel.launchTime);
                    break;
            }

            IEnumerator<CNCCommNetVessel> itr = sortedVessels.GetEnumerator();
            while(itr.MoveNext())
            {
                CNCCommNetVessel thisVessel = itr.Current;
                if (MapViewFiltering.CheckAgainstFilter(thisVessel.Vessel))
                    newRows.Add(createVesselRow(thisVessel));
            }

            /*
            for (int i=0; i< allVessels.Count; i++)
            {
                if (MapViewFiltering.CheckAgainstFilter(allVessels[i].Vessel))
                    newRows.Add(createVesselRow(allVessels[i]));
            }
            */

            return newRows;
        }

        /////////////////////
        // Actions
        /// <summary>
        /// Action to update the frequency of the vessel
        /// </summary>
        private void updateVessel(Vessel updatedVessel, short previousFrequency)
        {
            if (updatedVessel == null)
                return;

            short vesselFrequency = (updatedVessel.connection as CNCCommNetVessel).getRadioFrequency();

            updateVesselGUIRow(updatedVessel);
            updateConstellationGUIRow(vesselFrequency, -1);
            updateConstellationGUIRow(previousFrequency, -1);
        }
    }
}
