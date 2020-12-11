using CommNet;
using CommNetConstellation.CommNetLayer;
using CommNetConstellation.UI.DialogGUI;
using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Tree structure class for data
    /// </summary>
    #region TreeEntry class region
    public class TreeEntry
    {
        public String Text { get; set; }
        public Guid Guid { get; set; }
        public Color Color;
        public List<TreeEntry> SubEntries { get; private set; }
        public bool Expanded { get; set; }
        public int Depth { get; set; }

        public TreeEntry()
        {
            SubEntries = new List<TreeEntry>();
            Guid = Guid.Empty;
            Expanded = true;
            Text = "Root";
        }

        public void Clear()
        {
            for (int i = 0; i < SubEntries.Count; i++)
            {
                SubEntries[i].Clear();
            }
            SubEntries.Clear();
            Expanded = true;
            Depth = 0;
        }

        public void ComputeDepth(int d = 0)
        {
            Depth = d++;
            for (int i = 0; i < SubEntries.Count; i++)
            {
                SubEntries[i].ComputeDepth(d);
            }
        }
    }
    #endregion

    /// <summary>
    /// Interact with constellations or vessels (Controller)
    /// </summary>
    public class ConstellationControlDialog : AbstractDialog
    {
        public enum VesselListSort {LAUNCHDATE_ASC, LAUNCHDATE_DESC, RADIOFREQ, VESSELNAME, CBODY };
        private enum ContentType { CONSTELLATIONS, GROUNDSTATIONS, VESSELS };

        private readonly short depthWidth = 40;
        private readonly short colorWidth = 300;
        private readonly short headerWidth = 560;

        private static readonly Texture2D colorTexture = UIUtils.loadImage("colorDisplay");
        private static readonly Texture2D focusTexture = UIUtils.loadImage("focusEye");

        private ContentType currentContentType;
        private DialogGUIVerticalLayout contentLayout;
        private VesselListSort currentVesselSort;
        private DialogGUIHorizontalLayout sortVesselBtnLayout;
        private CustomDialogGUIScrollList scrollArea;

        private TreeEntry celestialBodyTree = new TreeEntry();
        private Dictionary<CelestialBody, TreeEntry> celestialBodyDict = new Dictionary<CelestialBody, TreeEntry>();
        private TreeEntry constellationTree = new TreeEntry();
        private Dictionary<short, TreeEntry> constellationDict = new Dictionary<short, TreeEntry>();

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
        #region CONSTELLATIONS region

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
        #endregion

        /////////////////////
        // VESSELS
        /////////////////////
        #region VESSELS region

        /////////////////////
        // Main GUI
        #region Main GUI region
        /// <summary>
        /// Main component of vessel interface
        /// </summary>
        private List<DialogGUIBase> getVesselContentLayout()
        {
            if(constellationDict.Keys.Count <= 0)
            {
                constellationTree.Clear();
                constellationDict.Clear();

                AddConstellations(constellationTree, constellationDict);
                AddVessels(constellationTree, constellationDict);
            }

            if (celestialBodyDict.Keys.Count <= 0)
            {
                celestialBodyTree.Clear();
                celestialBodyDict.Clear();

                AddCelestialBodies(celestialBodyTree, celestialBodyDict);
                AddVessels(celestialBodyTree, celestialBodyDict);

                celestialBodyTree.ComputeDepth();
            }

            List<DialogGUIBase> vesselComponments = new List<DialogGUIBase>();
            vesselComponments.AddRange(getVesselContentRows(VesselListSort.CBODY));
            return vesselComponments;
        }

        /// <summary>
        /// Bottom component of vessel interface
        /// </summary>
        private DialogGUIBase[] getVesselSortLayout()
        {
            float btnWidth = 100;
            float btnHeight = 28;
            UIStyle style = new UIStyle();
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;

            DialogGUILabel sortLabel = new DialogGUILabel(Localizer.Format("#CNC_ConstellationControl_sortLabel"), style);//"Sort by"
            DialogGUIButton launchSortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_launchSortBtn"), delegate { currentVesselSort = currentVesselSort == VesselListSort.LAUNCHDATE_DESC? VesselListSort.LAUNCHDATE_ASC : VesselListSort.LAUNCHDATE_DESC; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);//"Launch time"
            DialogGUIButton freqSortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_freqSortBtn"), delegate { currentVesselSort = VesselListSort.RADIOFREQ; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth+40, btnHeight, false);//"Strongest frequency"
            //DialogGUIButton nameSortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_nameSortBtn"), delegate { currentVesselSort = VesselListSort.VESSELNAME; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);//"Vessel name"
            DialogGUIButton bodySortBtn = new DialogGUIButton(Localizer.Format("#CNC_ConstellationControl_bodySortBtn"), delegate { currentVesselSort = VesselListSort.CBODY; mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, btnWidth, btnHeight, false);//"Celestial body"

            return new DialogGUIBase[] { sortLabel, launchSortBtn, freqSortBtn, bodySortBtn };
        }

        /// <summary>
        /// Content for vessel interface
        /// </summary>
        private List<DialogGUIHorizontalLayout> getVesselContentRows(VesselListSort sorting)
        {
            currentVesselSort = sorting;

            switch (sorting)
            {
                case VesselListSort.CBODY:
                    return populateCBodyVesselRows(sorting);
                case VesselListSort.LAUNCHDATE_ASC:
                case VesselListSort.LAUNCHDATE_DESC:
                    return populateLaunchVesselRows(sorting);
                case VesselListSort.RADIOFREQ:
                    return populateFreqVesselRows(sorting);
                case VesselListSort.VESSELNAME:
                    CNCLog.Verbose("VesselListSort.VESSELNAME not in use");
                    return new List<DialogGUIHorizontalLayout>();
                default:
                    CNCLog.Verbose("Unknown VesselListSort");
                    return new List<DialogGUIHorizontalLayout>();
            }
        }

        /// <summary>
        /// Common content row for vessel
        /// </summary>
        private DialogGUIHorizontalLayout createVesselRow(CNCCommNetVessel vessel, int depth = 0)
        {
            //answer is from FlagBrowserGUIButton
            DialogGUIImage focusImage = new DialogGUIImage(new Vector2(32f, 32f), Vector2.zero, Color.white, focusTexture);
            DialogGUIHorizontalLayout imageBtnLayout = new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { focusImage });
            DialogGUIButton focusButton = new DialogGUIButton("", delegate { vesselFocusClick(vessel.Vessel); }, 32, 32, false, new DialogGUIBase[] { imageBtnLayout });

            DialogGUILabel vesselLabel = new DialogGUILabel(vessel.Vessel.GetDisplayName(), 160, 12);
            DialogGUILabel freqLabel = new DialogGUILabel(getFreqString(vessel.getFrequencyList(), vessel.getStrongestFrequency()), 160, 12);
            DialogGUIButton setupButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_Setupbutton"), delegate { vesselSetupClick(vessel.Vessel); }, 70, 32, false);//"Setup"

            DialogGUIHorizontalLayout vesselGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUISpace(depth * depthWidth), focusButton, vesselLabel, freqLabel, setupButton, new DialogGUIFlexibleSpace() });
            vesselGroup.SetOptionText(vessel.Vessel.id.ToString());
            return vesselGroup;
        }
        #endregion

        /////////////////////
        // Sub GUI - vessels sorted by planets
        #region Sub GUI - vessels sorted by planets region
        private List<DialogGUIHorizontalLayout> populateCBodyVesselRows(VesselListSort sorting)
        {
            List<DialogGUIHorizontalLayout> newRows = new List<DialogGUIHorizontalLayout>();
            List<CNCCommNetVessel> allVessels = CNCCommNetScenario.Instance.getCommNetVessels();
            allVessels.Sort((x, y) => x.Vessel.GetName().CompareTo(y.Vessel.GetName()));

            // find paths touched by vessels
            List<TreeEntry> pathDict = new List<TreeEntry>();
            List<List<TreeEntry>> paths = new List<List<TreeEntry>>();
            findAllPaths(celestialBodyTree, new List<TreeEntry>(), paths);
            for(int i=0; i<paths.Count;i++)
            {
                for(int j=0;j<paths[i].Count; j++)
                {
                    if (!pathDict.Contains(paths[i][j]))
                    {
                        pathDict.Add(paths[i][j]);
                    }
                }
            }

            // Depth-first tree traversal.
            Stack<TreeEntry> dfs = new Stack<TreeEntry>();
            for (int i = 0; i < celestialBodyTree.SubEntries.Count; i++)
            {
                dfs.Push(celestialBodyTree.SubEntries[i]);
            }

            while (dfs.Count > 0)
            {
                var current = dfs.Pop();

                // push childen if expanded
                if (current.Expanded)
                {
                    for (int j = 0; j < current.SubEntries.Count; j++)
                    {
                        dfs.Push(current.SubEntries[j]);
                    }
                }

                // draw node
                if (current.Guid == Guid.Empty) // planet
                {
                    var body = FlightGlobals.Bodies.Find(x => x.bodyName.Equals(current.Text));
                    if (body != null && pathDict.Contains(current))
                    {
                        newRows.Add(createBodyHeaderRow(current.Depth - 1, current.Expanded, current.Color, body));
                    }
                }
                else
                {
                    var vessel = allVessels.Find(x => x.Vessel.id == current.Guid);
                    if (vessel != null && MapViewFiltering.CheckAgainstFilter(vessel.Vessel))
                    {
                        newRows.Add(createVesselRow(vessel, current.Depth - 1));
                    }
                }
            }

            return newRows;
        }

        private void findAllPaths(TreeEntry node, List<TreeEntry> prefixPath, List<List<TreeEntry>> paths)
        {
            var planetEndPathAdded = false;
            List<TreeEntry> nextPath = new List<TreeEntry>(prefixPath);
            nextPath.Add(node);
            for (int i = 0; i < node.SubEntries.Count; i++)
            {
                if(node.SubEntries[i].Guid != Guid.Empty) //child is vessel
                {
                    var vessel = CNCCommNetScenario.Instance.getCommNetVessels().Find(x => x.Vessel.id == node.SubEntries[i].Guid);
                    if (vessel != null && MapViewFiltering.CheckAgainstFilter(vessel.Vessel) && !planetEndPathAdded)
                    {
                        paths.Add(nextPath);
                        planetEndPathAdded = true; //add once to avoid duplicate planet-end paths for all vessels
                    }
                }
                else
                {
                    findAllPaths(node.SubEntries[i], nextPath, paths);
                }
            }
        }

        private DialogGUIHorizontalLayout createBodyHeaderRow(int depth, bool expanded, Color color, CelestialBody body)
        {
            Texture2D texture = UIUtils.createAndColorize(20, 10, color);

            UIStyle style = new UIStyle();
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 14;
            style.alignment = TextAnchor.MiddleLeft;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;
            DialogGUILabel bodyLabel = new DialogGUILabel((expanded ? ">" : "<"), style);
            DialogGUILabel nameLabel = new DialogGUILabel(body.bodyName, style);
            
            DialogGUIImage bodyImage = new DialogGUIImage(new Vector2(colorWidth + 50 - depth * depthWidth, 10f), Vector2.zero, Color.white, texture);
            DialogGUIHorizontalLayout imageBtnLayout = new DialogGUIHorizontalLayout(TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUISpace(10), bodyLabel, new DialogGUISpace(10), nameLabel, new DialogGUIFlexibleSpace(), bodyImage, new DialogGUISpace(10) });
            DialogGUIButton expandButton = new DialogGUIButton("", delegate { toggleCBodyButton(body); mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, headerWidth - depth * depthWidth, 24, false, new DialogGUIBase[] { imageBtnLayout });
            DialogGUIHorizontalLayout bodyGroup = new DialogGUIHorizontalLayout(false, false, 0, new RectOffset(0, 0, 2, 2), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUISpace(depth * depthWidth), expandButton, new DialogGUIFlexibleSpace() });
            bodyGroup.SetOptionText(body.bodyName);
            return bodyGroup;
        }

        private void toggleCBodyButton(CelestialBody body)
        {
            if(celestialBodyDict.ContainsKey(body))
            {
                var treeEntry = celestialBodyDict[body];
                treeEntry.Expanded = !treeEntry.Expanded;
            }
        }

        private void AddCelestialBodies(TreeEntry tree, Dictionary<CelestialBody, TreeEntry> dict)
        {
            for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                var cb = FlightGlobals.Bodies[i];
                if (!dict.ContainsKey(cb))
                {
                    dict[cb] = new TreeEntry();
                }

                var current = dict[cb];
                current.Text = cb.bodyName;
                current.Guid = Guid.Empty;
                current.Color = cb.GetOrbitDriver() != null ? cb.GetOrbitDriver().Renderer.nodeColor : Color.yellow;
                current.Color.a = 1.0f;

                // have moons?
                if (cb.referenceBody != cb)
                {
                    CelestialBody parent = cb.referenceBody;
                    if (!dict.ContainsKey(parent))
                    {
                        dict[parent] = new TreeEntry();
                    }
                    dict[parent].SubEntries.Add(current);
                }
                else
                {
                    tree.SubEntries.Add(current);
                }
            }

            // Sort the lists based on semi-major axis. In reverse because of how we render it.
            for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                var cb = FlightGlobals.Bodies[i];
                dict[cb].SubEntries.Sort((b, a) =>
                {
                    return FlightGlobals.Bodies.Find(x => x.bodyName.Equals(a.Text)).orbit.semiMajorAxis.CompareTo(
                        FlightGlobals.Bodies.Find(x => x.bodyName.Equals(b.Text)).orbit.semiMajorAxis);
                });
            }
            tree.SubEntries.Reverse();
        }

        private void AddVessels(TreeEntry tree, Dictionary<CelestialBody, TreeEntry> dict)
        {
            List<CNCCommNetVessel> allVessels = CNCCommNetScenario.Instance.getCommNetVessels();

            for (int i = 0; i < allVessels.Count; i++)
            {
                var thisVessel = allVessels[i];
                TreeEntry current = new TreeEntry()
                {
                    Text = thisVessel.Vessel.vesselName,
                    Guid = thisVessel.Vessel.id,
                    Color = Color.white,
                };
                dict[thisVessel.Vessel.mainBody].SubEntries.Add(current);
            }
        }
        #endregion

        /////////////////////
        // Sub GUI - vessels sorted by frequencies
        #region Sub GUI - vessels sorted by frequencies region
        private List<DialogGUIHorizontalLayout> populateFreqVesselRows(VesselListSort sorting)
        {
            List<DialogGUIHorizontalLayout> newRows = new List<DialogGUIHorizontalLayout>();
            List<CNCCommNetVessel> allVessels = CNCCommNetScenario.Instance.getCommNetVessels();
            allVessels.Sort((x, y) => x.Vessel.GetName().CompareTo(y.Vessel.GetName()));

            // Depth-first tree traversal.
            Stack<TreeEntry> dfs = new Stack<TreeEntry>();
            for (int i = 0; i < constellationTree.SubEntries.Count; i++)
            {
                dfs.Push(constellationTree.SubEntries[i]);
            }

            while (dfs.Count > 0)
            {
                var current = dfs.Pop();
                var hasChildrenPermitted = false;

                // peek children
                for (int j = 0; j < current.SubEntries.Count; j++)
                {
                    var vessel = allVessels.Find(x => x.Vessel.id == current.SubEntries[j].Guid);
                    if (vessel != null && MapViewFiltering.CheckAgainstFilter(vessel.Vessel))
                    {
                        hasChildrenPermitted = true;
                        break;
                    }
                }

                // push children if expanded
                if (current.Expanded)
                {
                    for (int j = 0; j < current.SubEntries.Count; j++)
                    {
                        dfs.Push(current.SubEntries[j]);
                    }
                }

                // draw node
                var constellation = CNCCommNetScenario.Instance.constellations.Find(x => ("freq"+x.frequency).Equals(current.Text));
                if (constellation != null && hasChildrenPermitted)
                {
                    newRows.Add(createFreqHeaderRow(current.Expanded, current.Color, constellation));
                }
                else
                {
                    var vessel = allVessels.Find(x => x.Vessel.id == current.Guid);
                    if (vessel != null && MapViewFiltering.CheckAgainstFilter(vessel.Vessel))
                    {
                        newRows.Add(createVesselRow(vessel));
                    }
                }
            }

            return newRows;
        }

        private void AddConstellations(TreeEntry tree, Dictionary<short, TreeEntry> dict)
        {
            List<Constellation> allConstellations = CNCCommNetScenario.Instance.constellations;
            allConstellations.Sort((x, y) => y.frequency - x.frequency);

            for (int i = 0; i < allConstellations.Count; i++)
            {
                var con = allConstellations[i];
                if (!dict.ContainsKey(con.frequency))
                {
                    dict[con.frequency] = new TreeEntry();
                }

                var current = dict[con.frequency];
                current.Text = "freq"+con.frequency;
                current.Guid = Guid.Empty;
                current.Color = con.color;
                current.Color.a = 1.0f;

                tree.SubEntries.Add(current);
            }
        }

        private void AddVessels(TreeEntry tree, Dictionary<short, TreeEntry> dict)
        {
            List<CNCCommNetVessel> allVessels = CNCCommNetScenario.Instance.getCommNetVessels();

            for (int i = 0; i < allVessels.Count; i++)
            {
                var thisVessel = allVessels[i];
                TreeEntry current = new TreeEntry()
                {
                    Text = thisVessel.Vessel.vesselName,
                    Guid = thisVessel.Vessel.id,
                    Color = Color.white,
                };
                dict[thisVessel.getStrongestFrequency()].SubEntries.Add(current);
            }
        }

        private DialogGUIHorizontalLayout createFreqHeaderRow(bool expanded, Color color, Constellation constellation)
        {
            Texture2D texture = UIUtils.createAndColorize(20, 10, color);

            UIStyle style = new UIStyle();
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 14;
            style.alignment = TextAnchor.MiddleLeft;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;
            DialogGUILabel bodyLabel = new DialogGUILabel((expanded ? ">" : "<"), style);
            DialogGUILabel nameLabel = new DialogGUILabel(String.Format("{0} {2} - {1}", Localizer.Format("#CNC_Generic_FrequencyLabel"), constellation.name, constellation.frequency), style);

            DialogGUIImage bodyImage = new DialogGUIImage(new Vector2(colorWidth - 50, 10f), Vector2.zero, Color.white, texture);
            DialogGUIHorizontalLayout imageBtnLayout = new DialogGUIHorizontalLayout(TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUISpace(10), bodyLabel, new DialogGUISpace(10), nameLabel, new DialogGUIFlexibleSpace(), bodyImage, new DialogGUISpace(10) });
            DialogGUIButton expandButton = new DialogGUIButton("", delegate { toggleConstellationButton(constellation); mapfilterChanged(MapViewFiltering.vesselTypeFilter); }, headerWidth, 24, false, new DialogGUIBase[] { imageBtnLayout });
            DialogGUIHorizontalLayout bodyGroup = new DialogGUIHorizontalLayout(false, false, 0, new RectOffset(0, 0, 2, 2), TextAnchor.MiddleLeft, new DialogGUIBase[] { expandButton, new DialogGUIFlexibleSpace() });
            bodyGroup.SetOptionText(constellation.frequency + "");
            return bodyGroup;
        }

        private void toggleConstellationButton(Constellation constellation)
        {
            if (constellationDict.ContainsKey(constellation.frequency))
            {
                var treeEntry = constellationDict[constellation.frequency];
                treeEntry.Expanded = !treeEntry.Expanded;
            }
        }
        #endregion

        /////////////////////
        // Sub GUI - vessels sorted by launch time
        #region Sub GUI - vessels sorted by launch time region
        private List<DialogGUIHorizontalLayout> populateLaunchVesselRows(VesselListSort sorting)
        {
            List<DialogGUIHorizontalLayout> newRows = new List<DialogGUIHorizontalLayout>();
            List<CNCCommNetVessel> allVessels = CNCCommNetScenario.Instance.getCommNetVessels();

            if (sorting == VesselListSort.LAUNCHDATE_DESC)
            {
                allVessels.Sort((x, y) => y.Vessel.launchTime.CompareTo(x.Vessel.launchTime));
            }
            if (sorting == VesselListSort.LAUNCHDATE_ASC)
            {
                allVessels.Sort((x, y) => x.Vessel.launchTime.CompareTo(y.Vessel.launchTime));
            }

            for (int i = 0; i < allVessels.Count; i++)
            {
                var vessel = allVessels[i];
                if (vessel != null && MapViewFiltering.CheckAgainstFilter(vessel.Vessel))
                {
                    newRows.Add(createVesselRow(vessel));
                }
            }

            return newRows;
        }
        #endregion

        /////////////////////
        // Actions
        #region Actions region
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
                    DialogGUILabel freqLabel = thisRow.children[3] as DialogGUILabel;
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

            //clear content rows
            List<DialogGUIBase> rows = contentLayout.children;
            for (int i = rows.Count - 1; i >= 1; i--)
            {
                DialogGUIBase thisRow = rows[i];
                rows.RemoveAt(i);
                thisRow.uiItem.gameObject.DestroyGameObjectImmediate(); // necessary to free memory up
            }

            //generate content rows
            List<DialogGUIHorizontalLayout> newRows = getVesselContentRows(currentVesselSort);
            Stack<Transform> stack = new Stack<Transform>(); // some data on hierarchy of GUI components
            stack.Push(contentLayout.uiItem.gameObject.transform); // need the reference point of the parent GUI component for position and size
            for (int i = 0; i < newRows.Count; i++)
            {
                newRows[i].Create(ref stack, HighLogic.UISkin); // required to force the GUI creation
                rows.Add(newRows[i]);
            }
        }
        #endregion

        #endregion

        /////////////////////
        // GROUND STATIONS
        /////////////////////

        #region GROUND STATIONS region
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
        #endregion
    }
}
