using CommNet;
using CommNetConstellation.CommNetLayer;
using KSP.Localization;
using System.Collections.Generic;
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
        private enum ContentType { CONSTELLATIONS, GROUNDSTATIONS, VESSELS };

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private static readonly Texture2D focusTexture = UIUtils.loadImage("focusEye");
        private static readonly Texture2D groundstationTexture = UIUtils.loadImage("groundStationMark");
        private UIStyle focusImageButtonStyle = null;

        private ContentType currentContentType;
        private DialogGUIVerticalLayout contentLayout;
        private VesselListSort currentVesselSort;
        private DialogGUIHorizontalLayout sortVesselBtnLayout;
        

        public ConstellationControlDialog(string title) : base("CNCControl",
                                                            title, 
                                                            0.8f, //x
                                                            0.5f, //y
                                                            600, //width
                                                            450, //height
                                                            new DialogOptions[] { DialogOptions.ShowVersion, DialogOptions.HideCloseButton, DialogOptions.AllowBgInputs }) //arguments
        {
            
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            try
            {
                focusImageButtonStyle = UIUtils.createImageButtonStyle(focusTexture);
            }
            catch (UnityException e) // temp workaround for Mac players because the focus texture is somehow made unreadable by unknown force on Mac only
            {
                CNCLog.Error("Texture \"{0}\" for a image button is unreadable. A text button is used instead.", focusTexture.ToString());
                focusImageButtonStyle = null;
            }

            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUILabel("Manage communication networks of ground, air and space vessels.", false, false));

            float btnWidth = (600-50)/3;
            float btnHeight = 32;
            DialogGUIButton constellationBtn = new DialogGUIButton("Constellations", delegate { displayContentLayout(ContentType.CONSTELLATIONS); }, btnWidth, btnHeight, false);
            DialogGUIButton groundstationBtn = new DialogGUIButton("Ground Stations", delegate { displayContentLayout(ContentType.GROUNDSTATIONS); }, btnWidth, btnHeight, false);
            DialogGUIButton vesselBtn = new DialogGUIButton("CommNet Vessels", delegate { displayContentLayout(ContentType.VESSELS); }, btnWidth, btnHeight, false);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { constellationBtn, groundstationBtn, vesselBtn}));

            contentLayout = new DialogGUIVerticalLayout(true, false, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) });
            contentLayout.AddChildren(getVesselContentLayout().ToArray());
            this.currentContentType = ContentType.VESSELS;
            listComponments.Add(new DialogGUIScrollList(new Vector2(550, 250), false, true, contentLayout));

            sortVesselBtnLayout = new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, getVesselSortLayout());
            listComponments.Add(sortVesselBtnLayout);

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

        private void displayContentLayout(ContentType type)
        {
            deregisterLayoutComponents(contentLayout);
            deregisterLayoutComponents(sortVesselBtnLayout);
            switch (type)
            {
                case ContentType.CONSTELLATIONS:
                    contentLayout.AddChildren(getConstellationContentLayout().ToArray());
                    break;
                case ContentType.GROUNDSTATIONS:
                    contentLayout.AddChildren(getGroundstationContentLayout().ToArray());
                    break;
                case ContentType.VESSELS:
                    contentLayout.AddChildren(getVesselContentLayout().ToArray());
                    sortVesselBtnLayout.AddChildren(getVesselSortLayout());
                    break;
            }
            this.currentContentType = type;
            registerLayoutComponents(contentLayout);
            registerLayoutComponents(sortVesselBtnLayout);
        }

        /////////////////////
        // CONSTELLATIONS
        /////////////////////

        /////////////////////
        // GUI
        private List<DialogGUIBase> getConstellationContentLayout()
        {
            List<DialogGUIBase> constellationComponments = new List<DialogGUIBase>();

            DialogGUIButton createButton = new DialogGUIButton("New constellation", newConstellationClick, false);
            DialogGUIHorizontalLayout creationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), createButton, new DialogGUIFlexibleSpace() });
            constellationComponments.Add(creationGroup);

            for (int i = 0; i < CNCCommNetScenario.Instance.constellations.Count; i++)
                constellationComponments.Add(createConstellationRow(CNCCommNetScenario.Instance.constellations[i]));

            return constellationComponments;
        }

        private DialogGUIHorizontalLayout createConstellationRow(Constellation thisConstellation)
        {
            Color color = Constellation.getColor(thisConstellation.frequency);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, thisConstellation.color, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(thisConstellation.name, 160, 12);
            DialogGUILabel freqLabel = new DialogGUILabel(string.Format("Frequency: <color={0}>{1}</color>", UIUtils.colorToHex(color), thisConstellation.frequency), 120, 12);
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
            if (this.currentContentType != ContentType.CONSTELLATIONS)
                return -1;

            List<DialogGUIBase> rows = contentLayout.children;

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
            if (this.currentContentType != ContentType.CONSTELLATIONS)
                return;

            List<DialogGUIBase> rows = contentLayout.children;

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

                short publicFrequency = CNCSettings.Instance.PublicRadioFrequency;

                List<CNCCommNetVessel> affectedVessels = CNCCommNetScenario.Instance.getCommNetVessels().FindAll(x => x.getFrequencies().Contains(deletedConstellation.frequency));
                for (int i = 0; i < affectedVessels.Count; i++)
                {
                    affectedVessels[i].replaceAllFrequencies(deletedConstellation.frequency, publicFrequency);
                    affectedVessels[i].OnAntennaChange();
                }

                if(affectedVessels.Count > 0)
                {
                    updateConstellationGUIRow(publicFrequency, -1);
                }

                List<CNCCommNetHome> affectedStations = CNCCommNetScenario.Instance.groundStations.FindAll(x => x.Frequencies.Contains(deletedConstellation.frequency));
                for (int i = 0; i < affectedStations.Count; i++)
                {
                    affectedStations[i].deleteFrequency(deletedConstellation.frequency);
                }
            }
        }

        /// <summary>
        /// Action to create a new constellation and save it
        /// </summary>
        private void createNewConstellation(Constellation newConstellation)
        {
            DialogGUIHorizontalLayout newConstellationGUIRow = createConstellationRow(newConstellation);
            contentLayout.AddChild(newConstellationGUIRow);

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(contentLayout.uiItem.gameObject.transform); // transform effect: new row goes to the end of the list 
            newConstellationGUIRow.Create(ref stack, HighLogic.UISkin);
        }

        /// <summary>
        /// Action to change the existing constellation
        /// </summary>
        private void updateConstellation(Constellation updatedConstellation, short previousFrequency)
        {
            updateConstellationGUIRow(updatedConstellation.frequency, previousFrequency);
        }

        /////////////////////
        // VESSELS
        /////////////////////

        /////////////////////
        // GUI
        private List<DialogGUIBase> getVesselContentLayout()
        {
            currentVesselSort = VesselListSort.LAUNCHDATE;

            List<DialogGUIBase> vesselComponments = new List<DialogGUIBase>();
            List<DialogGUIHorizontalLayout> rows = populateVesselRows(MapViewFiltering.vesselTypeFilter);
            for (int i = 0; i < rows.Count; i++)
            {
                vesselComponments.Add(rows[i]);
            }

            return vesselComponments;
        }

        private DialogGUIBase[] getVesselSortLayout()
        {
            float btnWidth = 100;
            float btnHeight = 28;

            DialogGUILabel sortLabel = new DialogGUILabel("Sort by", 35, 12);
            DialogGUIButton launchSortBtn = new DialogGUIButton("Launch time", delegate { currentVesselSort = VesselListSort.LAUNCHDATE; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);
            DialogGUIButton freqSortBtn = new DialogGUIButton("Strongest frequency", delegate { currentVesselSort = VesselListSort.RADIOFREQ; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth+40, btnHeight, false);
            DialogGUIButton nameSortBtn = new DialogGUIButton("Vessel name", delegate { currentVesselSort = VesselListSort.VESSELNAME; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);
            DialogGUIButton bodySortBtn = new DialogGUIButton("Celestial body", delegate { currentVesselSort = VesselListSort.CBODY; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);

            return new DialogGUIBase[] { sortLabel, launchSortBtn, freqSortBtn, nameSortBtn, bodySortBtn };
        }

        private DialogGUIHorizontalLayout createVesselRow(CNCCommNetVessel thisVessel)
        {
            DialogGUIButton focusButton;
            if(focusImageButtonStyle != null)
            {
                focusButton = new DialogGUIButton("", delegate { vesselFocusClick(thisVessel.Vessel); }, null, 32, 32, false, focusImageButtonStyle);
                focusButton.image = focusImageButtonStyle.normal.background;
            }
            else
            {
                focusButton = new DialogGUIButton("Focus", delegate { vesselFocusClick(thisVessel.Vessel); }, null, 32, 32, false);
            }

            DialogGUILabel vesselLabel = new DialogGUILabel(thisVessel.Vessel.GetDisplayName(), 160, 12);
            DialogGUILabel freqLabel = new DialogGUILabel(getFreqString(thisVessel.getFrequencies(), thisVessel.getStrongestFrequency()), 160, 12);
            DialogGUILabel locationLabel = new DialogGUILabel(Localizer.Format("Orbiting: <<1>>", thisVessel.Vessel.mainBody.GetDisplayName()), 100, 12);
            DialogGUIButton setupButton = new DialogGUIButton("Setup", delegate { vesselSetupClick(thisVessel.Vessel); }, 70, 32, false);

            DialogGUIHorizontalLayout vesselGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { focusButton, vesselLabel, freqLabel, locationLabel, setupButton });
            vesselGroup.SetOptionText(thisVessel.Vessel.id.ToString());
            return vesselGroup;
        }

        private void updateVesselGUIRow(Vessel updatedVessel)
        {
            if (this.currentContentType != ContentType.VESSELS)
                return;

            CNCCommNetVessel thisVessel = (CNCCommNetVessel)updatedVessel.Connection;
            List<DialogGUIBase> rows = contentLayout.children;

            for (int i = 0; i < rows.Count; i++)
            {
                DialogGUIBase thisRow = rows[i];
                if (thisRow.OptionText.Equals(updatedVessel.id.ToString()))
                {
                    DialogGUILabel freqLabel = thisRow.children[2] as DialogGUILabel;
                    freqLabel.SetOptionText(getFreqString(thisVessel.getFrequencies(), thisVessel.getStrongestFrequency()));
                    return;
                }
            }
        }

        private void vesselSetupClick(Vessel thisVessel)
        {
            new VesselSetupDialog("Vessel - <color=#00ff00>Setup</color>", thisVessel, updateVesselGUIRow).launch();
        }

        private void vesselFocusClick(Vessel thisVessel)
        {
            PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.AddTarget(thisVessel.mapObject));
            PlanetariumCamera.fetch.targets.Remove(thisVessel.mapObject);
        }

        private void mapfilterChanged(MapViewFiltering.VesselTypeFilter filter)
        {
            if (this.currentContentType != ContentType.VESSELS)
                return;

            //clear vessel rows
            List<DialogGUIBase> rows = contentLayout.children;
            for (int i = rows.Count-1; i >= 1 ; i--)
            {
                DialogGUIBase thisRow = rows[i];
                rows.RemoveAt(i);
                thisRow.uiItem.gameObject.DestroyGameObjectImmediate(); // necessary to free memory up
            }
            
            List<DialogGUIHorizontalLayout> newRows = populateVesselRows(filter);
            Stack<Transform> stack = new Stack<Transform>(); // some data on hierarchy of GUI components
            stack.Push(contentLayout.uiItem.gameObject.transform); // need the reference point of the parent GUI component for position and size
            for (int i = 0; i < newRows.Count; i++)
            {
                newRows[i].Create(ref stack, HighLogic.UISkin); // required to force the GUI creation
                rows.Add(newRows[i]);
            }
        }

        private List<DialogGUIHorizontalLayout> populateVesselRows(VesselTypeFilter filter)
        {
            List<DialogGUIHorizontalLayout> newRows = new List<DialogGUIHorizontalLayout>();
            List<CNCCommNetVessel> allVessels = CNCCommNetScenario.Instance.getCommNetVessels();

            switch (currentVesselSort)
            {
                case VesselListSort.RADIOFREQ:
                    allVessels.Sort((x, y) => x.getStrongestFrequency()-y.getStrongestFrequency());
                    break;
                case VesselListSort.VESSELNAME:
                    allVessels.Sort((x, y) => x.Vessel.GetName().CompareTo(y.Vessel.GetName()));
                    break;
                case VesselListSort.CBODY:
                    allVessels.Sort((x, y) => x.Vessel.mainBody.name.CompareTo(y.Vessel.mainBody.name));
                    break;
                default:
                    allVessels.Sort((x, y) => x.Vessel.launchTime.CompareTo(y.Vessel.launchTime));
                    break;
            }

            var itr = allVessels.GetEnumerator();
            while(itr.MoveNext())
            {
                CNCCommNetVessel thisVessel = itr.Current;
                if (MapViewFiltering.CheckAgainstFilter(thisVessel.Vessel))
                    newRows.Add(createVesselRow(thisVessel));
            }

            return newRows;
        }

        /////////////////////
        // GROUND STATIONS
        /////////////////////

        /////////////////////
        // GUI
        private List<DialogGUIBase> getGroundstationContentLayout()
        {
            List<DialogGUIBase> stationComponments = new List<DialogGUIBase>();

            //toggle button for ground station markers
            DialogGUIToggleButton toggleStationButton = new DialogGUIToggleButton(CNCCommNetScenario.Instance.hideGroundStations, "Hide all station markers", delegate (bool b) { CNCCommNetScenario.Instance.hideGroundStations = !CNCCommNetScenario.Instance.hideGroundStations; }, 60, 25);
            DialogGUIHorizontalLayout toggleStationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), toggleStationButton, new DialogGUIFlexibleSpace() });
            stationComponments.Add(toggleStationGroup);

            List<DialogGUIHorizontalLayout> rows = populateGroundStationRows();
            for (int i = 0; i < rows.Count; i++)
            {
                stationComponments.Add(rows[i]);
            }

            return stationComponments;
        }

        private List<DialogGUIHorizontalLayout> populateGroundStationRows()
        {
            List<DialogGUIHorizontalLayout> newRows = new List<DialogGUIHorizontalLayout>();
            List<CNCCommNetHome> stations = CNCCommNetScenario.Instance.groundStations;

            if (HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations)
            {
                for (int i = 0; i < stations.Count; i++)
                {
                    newRows.Add(createGroundStationRow(stations[i]));
                }
            }
            else
            {
                newRows.Add(createGroundStationRow(stations.Find(x => x.isKSC)));
            }

            return newRows;
        }

        private DialogGUIHorizontalLayout createGroundStationRow(CNCCommNetHome thisStation)
        {
            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(16, 16), Vector2.one, thisStation.Color, groundstationTexture);
            DialogGUILabel stationNameLabel = new DialogGUILabel(thisStation.stationName, 170, 12);
            DialogGUILabel locationLabel = new DialogGUILabel(string.Format("LAT: {0:0.0}\nLON: {1:0.0}", thisStation.latitude, thisStation.longitude), 100, 24);
            DialogGUILabel freqsLabel = new DialogGUILabel(getFreqString(thisStation.Frequencies), 210, 12);
            DialogGUIButton updateButton = new DialogGUIButton("Edit", delegate { groundstationEditClick(thisStation); }, 50, 32, false);

            DialogGUIBase[] rowGUIBase = new DialogGUIBase[] { colorImage, stationNameLabel, locationLabel, freqsLabel, updateButton };
            DialogGUIHorizontalLayout groundStationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, rowGUIBase);
            groundStationGroup.SetOptionText(thisStation.ID); //for quick identification
            return groundStationGroup;
        }

        private string getFreqString(List<short> frequencies, short strongestFreq = -1)
        {
            frequencies.Sort();
            string freqString = "Frequencies: ";

            if (frequencies.Count == 0) // nothing
                return "No frequency assigned";

            for (int i = 0; i < frequencies.Count; i++)
            {
                Color color = Constellation.getColor(frequencies[i]);
                freqString += string.Format("<color={0}>{1}</color>", UIUtils.colorToHex(color), (strongestFreq == frequencies[i])? "<b>"+ frequencies[i]+"</b>": frequencies[i] + "");
                if (i <= frequencies.Count - 2)
                    freqString += ", ";
            }

            return freqString;
        }

        private void updateGroundStationGUIRow(string stationID)
        {
            if (this.currentContentType != ContentType.GROUNDSTATIONS)
                return;

            List<DialogGUIBase> rows = contentLayout.children;

            for (int i = 1; i < rows.Count; i++)
            {
                DialogGUIBase thisRow = rows[i];
                if (thisRow.OptionText.Equals(stationID))
                {
                    DialogGUIImage colorImage = thisRow.children[0] as DialogGUIImage;
                    DialogGUILabel nameLabel = thisRow.children[1] as DialogGUILabel;
                    DialogGUILabel freqsLabel = thisRow.children[3] as DialogGUILabel;
                    CNCCommNetHome station = CNCCommNetScenario.Instance.groundStations.Find(x => x.ID.Equals(stationID));
                    colorImage.uiItem.GetComponent<RawImage>().color = station.Color;
                    nameLabel.SetOptionText(station.stationName);
                    freqsLabel.SetOptionText(getFreqString(station.Frequencies));

                    break;
                }
            }
        }

        /////////////////////
        // Actions
        private void groundstationEditClick(CNCCommNetHome thisStation)
        {
            new GroundStationEditDialog("Ground station - <color=#00ff00>Edit</color>", thisStation, updateGroundStationGUIRow).launch();
        }
    }
}
