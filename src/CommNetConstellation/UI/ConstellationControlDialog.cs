using CommNet;
using CommNetConstellation.CommNetLayer;
using CommNetConstellation.UI.DialogGUI;
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

        private ContentType currentContentType;
        private DialogGUIVerticalLayout contentLayout;
        private VesselListSort currentVesselSort;
        private DialogGUIHorizontalLayout sortVesselBtnLayout;
        private CustomDialogGUIScrollList scrollArea;

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
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUILabel(Localizer.Format("#CNC_ConstellationControl_listComponments"), false, false));//"Manage communication networks of ground, air and space vessels."

            float btnWidth = (600-50)/3;
            float btnHeight = 32;
            DialogGUIButton constellationBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_ConstellationBtn"), delegate { displayContentLayout(ContentType.CONSTELLATIONS); }, btnWidth, btnHeight, false);//"Constellations"
            DialogGUIButton groundstationBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_GroundstationBtn"), delegate { displayContentLayout(ContentType.GROUNDSTATIONS); }, btnWidth, btnHeight, false);//"Ground Stations"
            DialogGUIButton vesselBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_VesselBtn"), delegate { displayContentLayout(ContentType.VESSELS); }, btnWidth, btnHeight, false);//"CommNet Vessels"
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { constellationBtn, groundstationBtn, vesselBtn}));

            contentLayout = new DialogGUIVerticalLayout(true, false, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) });
            contentLayout.AddChildren(getVesselContentLayout().ToArray());
            this.currentContentType = ContentType.VESSELS;
            scrollArea = new CustomDialogGUIScrollList(new Vector2(550, 250), false, true, contentLayout);
            listComponments.Add(scrollArea);

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
            scrollArea.Resize();
        }

        /////////////////////
        // CONSTELLATIONS
        /////////////////////

        /////////////////////
        // GUI
        private List<DialogGUIBase> getConstellationContentLayout()
        {
            List<DialogGUIBase> constellationComponments = new List<DialogGUIBase>();

            DialogGUIButton createButton = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_createButton"), newConstellationClick, false);//"New constellation"
            DialogGUIToggleButton toggleOrbitButton = new DialogGUIToggleButton(CNCSettings.Instance.LegacyOrbitLineColor, Localizer.Format("#CNC_ConstellationControl_toggleOrbitButton"), delegate (bool b) { CNCSettings.Instance.LegacyOrbitLineColor = !CNCSettings.Instance.LegacyOrbitLineColor; }, 35, 25);//"Toggle colorized orbits"
            DialogGUIHorizontalLayout creationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), createButton, new DialogGUIFlexibleSpace() });
            DialogGUIHorizontalLayout toggleGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), toggleOrbitButton, new DialogGUIFlexibleSpace() });
            constellationComponments.Add(creationGroup);
            constellationComponments.Add(toggleGroup);

            for (int i = 0; i < CNCCommNetScenario.Instance.constellations.Count; i++)
                constellationComponments.Add(createConstellationRow(CNCCommNetScenario.Instance.constellations[i]));

            return constellationComponments;
        }

        private DialogGUIHorizontalLayout createConstellationRow(Constellation thisConstellation)
        {
            Color color = Constellation.getColor(thisConstellation.frequency);

            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.zero, thisConstellation.color, colorTexture);
            DialogGUILabel constNameLabel = new DialogGUILabel(thisConstellation.name, 170, 12);
            DialogGUILabel freqLabel = new DialogGUILabel(Localizer.Format("#CNC_Generic_FrequencyLabel") + string.Format(": <color={0}>{1}</color>", UIUtils.colorToHex(color), thisConstellation.frequency), 100, 12);//Frequency
            DialogGUILabel numSatsLabel = new DialogGUILabel(Localizer.Format("#CNC_ConstellationControl_numSatsLabel", Constellation.countVessels(thisConstellation)), 75, 12);//string.Format("{0} vessels", )
            DialogGUIButton updateButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_Editbutton"), delegate { editConstellationClick(thisConstellation); }, 50, 32, false);//"Edit"
            DialogGUIToggleButton toggleButton = new DialogGUIToggleButton(thisConstellation.visibility, Localizer.Format("#CNC_Generic_Mapbutton"), delegate { toggleConstellationVisibility(thisConstellation); }, 45, 32);//"Map"

            DialogGUIBase[] rowGUIBase = new DialogGUIBase[] { colorImage, constNameLabel, freqLabel, numSatsLabel, toggleButton, updateButton, null };
            if (thisConstellation.frequency == CNCSettings.Instance.PublicRadioFrequency)
                rowGUIBase[rowGUIBase.Length - 1] = new DialogGUIButton(Localizer.Format("#CNC_Generic_Resetbutton"), resetPublicConstClick, 60, 32, false);//"Reset"
            else
                rowGUIBase[rowGUIBase.Length - 1] = new DialogGUIButton(Localizer.Format("#CNC_Generic_DeleteButton"), delegate { deleteConstellationClick(thisConstellation); }, 60, 32, false);//"Delete"

            DialogGUIHorizontalLayout constellationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, rowGUIBase);
            constellationGroup.SetOptionText(thisConstellation.frequency.ToString()); //for quick identification
            return constellationGroup;
        }

        private void toggleConstellationVisibility(Constellation thisConstellation)
        {
            thisConstellation.visibility = !thisConstellation.visibility;
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
                    freqLabel.SetOptionText(Localizer.Format("#CNC_Generic_FrequencyLabel") + string.Format(": <color={0}>{1}</color>", UIUtils.colorToHex(updatedConstellation.color), updatedConstellation.frequency));//Frequency
                    vesselLabel.SetOptionText(Localizer.Format("#CNC_ConstellationControl_numSatsLabel", Constellation.countVessels(updatedConstellation)));// + " vessels"

                    thisRow.SetOptionText(updatedConstellation.frequency.ToString());
                    break;
                }
            }
        }

        private void resetPublicConstClick()
        {
            string message = Localizer.Format("#CNC_ConstellationControl_resetPublicMsg", CNCSettings.Instance.DefaultPublicName, UIUtils.colorToHex(CNCSettings.Instance.DefaultPublicColor));//string.Format("Revert to the default name '{0}' and color {1}?", 
            MultiOptionDialog warningDialog = new MultiOptionDialog("cncResetConstWindow", message, Localizer.Format("#CNC_ConstellationControl_Dialog_title"), HighLogic.UISkin, new DialogGUIBase[]//"Constellation"
            {
                new DialogGUIButton(Localizer.Format("#CNC_Generic_Resetbutton"), resetPublicConstellation, true),//"Reset"
                new DialogGUIButton(Localizer.Format("#CNC_Generic_CancelButton"), delegate { }, true)//"Cancel"
            });

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), warningDialog, false, HighLogic.UISkin, true, string.Empty);
        }

        private void deleteConstellationClick(Constellation thisConstellation)
        {
            string title = Localizer.Format("#CNC_ConstellationControl_DeleteDialog_title", thisConstellation.name);//string.Format("Deleting '{0}'?", )
            string message = Localizer.Format("#CNC_ConstellationControl_DeleteDialog_msg", thisConstellation.name);//string.Format("All the vessels of Constellation '{0}' will be reintegrated into the public constellation.", )

            MultiOptionDialog warningDialog = new MultiOptionDialog("cncDeleteConstWindow", message, title, HighLogic.UISkin, new DialogGUIBase[]
            {
                new DialogGUIButton(Localizer.Format("#CNC_Generic_DeleteButton"), delegate { deleteConstellation(thisConstellation); }, true),//"Delete"
                new DialogGUIButton(Localizer.Format("#CNC_Generic_CancelButton"), delegate { }, true)//"Cancel"
            });

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), warningDialog, false, HighLogic.UISkin, true);
        }

        private void newConstellationClick()
        {
            new ConstellationEditDialog(Localizer.Format("#CNC_ConstellationControl_NewConstellation_title"), null, createNewConstellation, null).launch();//"Constellation - <color=#00ff00>New</color>"
        }

        private void editConstellationClick(Constellation thisConstellation)
        {
            new ConstellationEditDialog(Localizer.Format("#CNC_ConstellationControl_EditConstellation_title"), thisConstellation, null, updateConstellation).launch();//"Constellation - <color=#00ff00>Edit</color>"
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

                List<CNCCommNetVessel> affectedVessels = CNCCommNetScenario.Instance.getCommNetVessels().FindAll(x => x.getFrequencyList().Contains(deletedConstellation.frequency));
                for (int i = 0; i < affectedVessels.Count; i++)
                {
                    affectedVessels[i].replaceAllFrequencies(deletedConstellation.frequency, publicFrequency);
                    affectedVessels[i].OnAntennaChange();
                }

                if(affectedVessels.Count > 0)
                {
                    updateConstellationGUIRow(publicFrequency, -1);
                }

                List<CNCCommNetHome> affectedStations = CNCCommNetScenario.Instance.groundStations.FindAll(x => x.getFrequencyList().Contains(deletedConstellation.frequency));
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

            DialogGUILabel sortLabel = new DialogGUILabel(Localizer.Format("#CNC_ConstellationControl_sortLabel"), 35, 12);//"Sort by"
            DialogGUIButton launchSortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_launchSortBtn"), delegate { currentVesselSort = VesselListSort.LAUNCHDATE; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);//"Launch time"
            DialogGUIButton freqSortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_freqSortBtn"), delegate { currentVesselSort = VesselListSort.RADIOFREQ; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth+40, btnHeight, false);//"Strongest frequency"
            DialogGUIButton nameSortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_nameSortBtn"), delegate { currentVesselSort = VesselListSort.VESSELNAME; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);//"Vessel name"
            DialogGUIButton bodySortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_bodySortBtn"), delegate { currentVesselSort = VesselListSort.CBODY; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);//"Celestial body"

            return new DialogGUIBase[] { sortLabel, launchSortBtn, freqSortBtn, nameSortBtn, bodySortBtn };
        }

        private DialogGUIHorizontalLayout createVesselRow(CNCCommNetVessel thisVessel)
        {
            //answer is from FlagBrowserGUIButton
            DialogGUIImage focusImage = new DialogGUIImage(new Vector2(32f, 32f), Vector2.zero, Color.white, focusTexture);
            DialogGUIHorizontalLayout imageBtnLayout = new DialogGUIHorizontalLayout(true, true, 0f, new RectOffset(1, 1, 1, 1), TextAnchor.MiddleCenter, new DialogGUIBase[]{ focusImage });
            DialogGUIButton focusButton= new DialogGUIButton("", delegate { vesselFocusClick(thisVessel.Vessel); }, 34, 34, false, new DialogGUIBase[] { imageBtnLayout });

            DialogGUILabel vesselLabel = new DialogGUILabel(thisVessel.Vessel.GetDisplayName(), 160, 12);
            DialogGUILabel freqLabel = new DialogGUILabel(getFreqString(thisVessel.getFrequencyList(), thisVessel.getStrongestFrequency()), 160, 12);
            DialogGUILabel locationLabel = new DialogGUILabel(Localizer.Format("#CNC_ConstellationControl_locationLabel", thisVessel.Vessel.mainBody.GetDisplayName()), 100, 12);//Orbiting: <<1>>
            DialogGUIButton setupButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_Setupbutton"), delegate { vesselSetupClick(thisVessel.Vessel); }, 70, 32, false);//"Setup"

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
                    freqLabel.SetOptionText(getFreqString(thisVessel.getFrequencyList(), thisVessel.getStrongestFrequency()));
                    return;
                }
            }
        }

        private void vesselSetupClick(Vessel thisVessel)
        {
            new VesselSetupDialog(Localizer.Format("#CNC_ConstellationControl_vesselSetup_title"), thisVessel, updateVesselGUIRow).launch();//"Vessel - <color=#00ff00>Setup</color>"
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
            DialogGUIToggleButton toggleStationButton = new DialogGUIToggleButton(CNCCommNetScenario.Instance.hideGroundStations, Localizer.Format("#CNC_ConstellationControl_toggleStationButton"), delegate (bool b) { CNCCommNetScenario.Instance.hideGroundStations = !CNCCommNetScenario.Instance.hideGroundStations; }, 60, 25);//"Hide all station markers"
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
            DialogGUIImage colorImage = new DialogGUIImage(new Vector2(16, 16), Vector2.one, thisStation.Color, CNCCommNetHome.getGroundStationTexture(thisStation.TechLevel));
            DialogGUILabel stationNameLabel = new DialogGUILabel(thisStation.stationName, 160, 12);
            DialogGUILabel locationLabel = new DialogGUILabel(Localizer.Format("#CNC_ConstellationControl_LatitudeAndLongitude", string.Format("{0:0.0}", thisStation.latitude), string.Format("{0:0.0}", thisStation.longitude)), 80, 24);//string.Format("LAT: \nLON: ", , )
            DialogGUILabel freqsLabel = new DialogGUILabel(getFreqString(thisStation.getFrequencyList()), 160, 12);
            DialogGUIButton buildButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_Upgradebutton"), delegate { groundstationBuildClick(thisStation); }, 70, 32, false);//"Upgrade"
            buildButton.OptionInteractableCondition = () => (thisStation.isKSC) ? false : (thisStation.TechLevel < 3) ? true : false;
            DialogGUIButton updateButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_Editbutton"), delegate { groundstationEditClick(thisStation); }, 50, 32, false);//"Edit"

            DialogGUIBase[] rowGUIBase = new DialogGUIBase[] { colorImage, stationNameLabel, locationLabel, freqsLabel, buildButton, updateButton };
            if (thisStation.isKSC) { rowGUIBase[4] = new DialogGUISpace(70); } //hide upgrade button

            DialogGUIHorizontalLayout groundStationGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, rowGUIBase);
            groundStationGroup.SetOptionText(thisStation.ID); //for quick identification
            return groundStationGroup;
        }

        private string getFreqString(List<short> frequencies, short strongestFreq = -1)
        {
            frequencies.Sort();
            string freqString = Localizer.Format("#CNC_ConstellationControl_getFreqString") + " ";//"Frequencies: "

            if (frequencies.Count == 0) // nothing
                return Localizer.Format("#CNC_ConstellationControl_getFreqString_nothing");//"No frequency assigned"

            for (int i = 0; i < frequencies.Count; i++)
            {
                Color color = Constellation.getColor(frequencies[i]);
                freqString += string.Format("<color={0}>{1}</color>", UIUtils.colorToHex(color), (strongestFreq == frequencies[i])? "<b>"+ frequencies[i]+"</b>": frequencies[i] + "");//
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
                    colorImage.uiItem.GetComponent<RawImage>().texture = CNCCommNetHome.getGroundStationTexture(station.TechLevel);
                    nameLabel.SetOptionText(station.stationName);
                    freqsLabel.SetOptionText(getFreqString(station.getFrequencyList()));

                    break;
                }
            }
        }

        /////////////////////
        // Actions
        private void groundstationEditClick(CNCCommNetHome thisStation)
        {
            new GroundStationEditDialog(Localizer.Format("#CNC_ConstellationControl_GroundStationEdit_title"), thisStation, updateGroundStationGUIRow).launch();//"Ground station - <color=#00ff00>Edit</color>"
        }

        private void groundstationBuildClick(CNCCommNetHome thisStation)
        {
            new GroundStationBuildDialog(Localizer.Format("#CNC_ConstellationControl_GroundStationBuild_title"), thisStation, updateGroundStationGUIRow).launch();//"Ground station - <color=#00ff00>Upgrade</color>"
        }
    }
}
