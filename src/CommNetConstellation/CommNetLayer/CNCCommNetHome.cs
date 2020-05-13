using CommNet;
using CommNetConstellation.UI;
using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Customise the home nodes
    /// </summary>
    public class CNCCommNetHome : CommNetHome, IComparable<CNCCommNetHome>
    {
        public static readonly Texture2D L0MarkTexture = UIUtils.loadImage("GroundStationL0Mark");
        public static readonly Texture2D L1MarkTexture = UIUtils.loadImage("GroundStationL1Mark");
        public static readonly Texture2D L2MarkTexture = UIUtils.loadImage("GroundStationL2Mark");
        public static readonly Texture2D L3MarkTexture = UIUtils.loadImage("GroundStationL3Mark");
        private static GUIStyle groundStationHeadline;

        private string stationInfoString = "";
        private Texture2D stationTexture;

        //to be saved to persistent.sfs
        [Persistent] public string ID;
        [Persistent] public Color Color = Color.red;
        [Persistent] protected string OptionalName = "";
        [Persistent] public short TechLevel = 0;
        [Persistent] public bool OverrideLatLongAlt = false;
        [Persistent] public double CustomLatitude = 0.0;
        [Persistent] public double CustomLongitude = 0.0;
        [Persistent] public double CustomAltitude = 0.0;
        [Persistent] public string CustomCelestialBody = "";
        [Persistent(collectionIndex = "Frequency")] protected List<short> Frequencies = new List<short>();

        //for low-gc operations
        protected short[] sorted_frequency_array;

        public double altitude { get { return this.alt; } set { this.alt = value; } }
        public double latitude { get { return this.lat; } set { this.lat = value; } }
        public double longitude { get { return this.lon; } set { this.lon = value; } }
        public CommNode commNode { get { return this.comm; } }
        public string stationName
        {
            get { return (this.OptionalName.Length == 0)? this.displaynodeName : this.OptionalName; }
            set { this.OptionalName = value; }
        }

        /// <summary>
        /// Empty constructor for ConfigNode.LoadObjectFromConfig()
        /// </summary>
        public CNCCommNetHome() { }

        public void copyOf(CommNetHome stockHome)
        {
            CNCLog.Verbose("Stock CommNet Home '{0}' added", stockHome.nodeName);

            this.ID = stockHome.nodeName;
            this.nodeName = stockHome.nodeName;
            this.displaynodeName = Localizer.Format(stockHome.displaynodeName);
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            this.body = stockHome.GetComponentInParent<CelestialBody>();

            //comm, lat, alt, lon are initialised by CreateNode() later
        }

        /// <summary>
        /// Apply the changes from persistent.sfs
        /// </summary>
        public void applySavedChanges(CNCCommNetHome stationSnapshot)
        {
            this.Color = stationSnapshot.Color;
            this.Frequencies = stationSnapshot.Frequencies;
            this.OptionalName = stationSnapshot.OptionalName;
            this.TechLevel = stationSnapshot.TechLevel;
            this.OverrideLatLongAlt = stationSnapshot.OverrideLatLongAlt;
            this.CustomLatitude = stationSnapshot.CustomLatitude;
            this.CustomLongitude = stationSnapshot.CustomLongitude;
            this.CustomAltitude = stationSnapshot.CustomAltitude;
            this.CustomCelestialBody = stationSnapshot.CustomCelestialBody;
        }

        /// <summary>
        /// Replace one specific frequency with new frequency
        /// </summary>
        public void replaceFrequency(short oldFrequency, short newFrequency)
        {
            this.Frequencies.Remove(oldFrequency);
            this.Frequencies.Add(newFrequency);
            this.Frequencies.Sort();
            regenerateFrequencyArray(this.Frequencies);
        }

        /// <summary>
        /// Drop the specific frequency from the list
        /// </summary>
        public void deleteFrequency(short frequency)
        {
            this.Frequencies.Remove(frequency);
            regenerateFrequencyArray(this.Frequencies);
        }

        /// <summary>
        /// Get the *sorted* array of frequencies only
        /// </summary>
        public short[] getFrequencyArray()
        {
            if(this.sorted_frequency_array == null)
            {
                regenerateFrequencyArray(this.Frequencies);
            }
            return this.sorted_frequency_array;
        }

        /// <summary>
        /// Get the *sorted* list of frequencies only
        /// </summary>
        public List<short> getFrequencyList()
        {
            return this.Frequencies;
        }

        /// <summary>
        /// Remove all frequencies
        /// </summary>
        public void deleteFrequencies()
        {
            this.Frequencies.Clear();
            regenerateFrequencyArray(this.Frequencies);
        }

        /// <summary>
        /// Replace all frequencies
        /// </summary>
        public void replaceFrequencies(List<short> newFreqs)
        {
            this.Frequencies = newFreqs;
            regenerateFrequencyArray(this.Frequencies);
        }

        /// <summary>
        /// Increment Tech Level Ground Station to max 3
        /// </summary>
        public void incrementTechLevel()
        {
            if (this.TechLevel < 3 && !this.isKSC)
            {
                this.TechLevel++;
                refresh();
            }
        }

        /// <summary>
        /// Decrement Tech Level Ground Station to min 0
        /// </summary>
        public void decrementTechLevel()
        {
            if (this.TechLevel > 0 && !this.isKSC)
            {
                this.TechLevel--;
                refresh();
            }
        }

        /// <summary>
        /// Set Tech Level Ground Station
        /// </summary>
        public void setTechLevel(short level)
        {
            if (level >= 0 && level <= 3 && !this.isKSC)
            {
                this.TechLevel = level;
                refresh();
            }
        }

        /// <summary>
        /// Update lat and long of celestial body
        /// </summary>
        public void setLatLongCoords(double lat, double lon, bool persistent = true)
        {
            this.OverrideLatLongAlt = persistent;
            this.latitude = this.CustomLatitude = lat;
            this.longitude = this.CustomLongitude = lon;
            refresh();
        }

        /// <summary>
        /// Update altitude on celestial body
        /// </summary>
        public void setAltitude(double alt, bool persistent = false)
        {
            this.OverrideLatLongAlt = persistent;
            this.altitude = this.CustomAltitude = alt;
            refresh();
        }

        /// <summary>
        /// Change how Start() runs
        /// </summary>
        protected override void Start()
        {
            if (groundStationHeadline == null)
            {
                groundStationHeadline = new GUIStyle(HighLogic.Skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = Color.yellow },
                    alignment = TextAnchor.MiddleCenter
                };
            }

            this.body = (this.CustomCelestialBody.Length > 0) ? FlightGlobals.Bodies.Find(x => x.name.Equals(this.CustomCelestialBody)) : base.GetComponentInParent<CelestialBody>();

            if(this.body == null)//one root cause is 3rd-party mod Making Less History, which disables 2 ground stations in Making History expansion
            {
                //self-destruct
                CNCLog.Error("CommNet Home '{0}' self-destructed due to missing info", this.ID);
                CNCCommNetScenario.Instance.groundStations.Remove(this);
                this.OnDestroy();
                UnityEngine.Object.Destroy(this);
                return;
            }

            if (this.nodeTransform == null)
            {
                this.nodeTransform = base.nodeTransform;
            }

            if (CommNetNetwork.Initialized)
            {
                this.OnNetworkInitialized();
            }

            GameEvents.CommNet.OnNetworkInitialized.Add(new EventVoid.OnEvent(this.OnNetworkInitialized));

            if (this.OverrideLatLongAlt)
            {
                this.latitude = this.CustomLatitude;
                this.longitude = this.CustomLongitude;
                this.altitude = this.CustomAltitude;
            }

            this.refresh();
        }

        /// <summary>
        /// Draw graphic components on screen like RemoteTech's ground-station marks
        /// </summary>
        public void OnGUI()
        {
            if (HighLogic.CurrentGame == null)
                return;

            if (!(HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.TRACKSTATION))
                return;

            if ((!HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations && !this.isKSC) || !MapView.MapIsEnabled || MapView.MapCamera == null)
                return;

            if (CNCCommNetScenario.Instance == null || CNCCommNetScenario.Instance.hideGroundStations)
                return;

            Vector3d worldPos = ScaledSpace.LocalToScaledSpace(this.comm.precisePosition);

            if (MapView.MapCamera.transform.InverseTransformPoint(worldPos).z < 0f)
                return;

            if (isOccluded(this.comm.precisePosition, this.body))
                return;

            if (!isOccluded(this.comm.precisePosition, this.body) && this.IsCamDistanceToWide(this.comm.precisePosition))
                return;

            //maths calculations
            var screenPosition = PlanetariumCamera.Camera.WorldToScreenPoint(worldPos);
            var centerPosition = new Vector3(screenPosition.x - 8, (Screen.height - screenPosition.y) - 8);
            var groundStationRect = new Rect(centerPosition.x, centerPosition.y, 16, 16);

            //draw the dot
            Color previousColor = GUI.color;
            GUI.color = this.Color;
            GUI.DrawTexture(groundStationRect, stationTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;

            //draw the headline above and below the dot
            if (UIUtils.ContainsMouse(groundStationRect))
            {
                Rect headlineRect = groundStationRect;

                //Name
                Vector2 nameDim = CNCCommNetHome.groundStationHeadline.CalcSize(new GUIContent(this.stationName));
                headlineRect.x -= nameDim.x/2 - 5;
                headlineRect.y -= nameDim.y + 5;
                headlineRect.width = nameDim.x;
                headlineRect.height = nameDim.y;
                GUI.Label(headlineRect, this.stationName, CNCCommNetHome.groundStationHeadline);

                //build station information
                if (this.TechLevel <= 0)
                {
                    stationInfoString = Localizer.Format("#CNC_CNCCommNetHome_nostation");//"Build a ground station";
                }
                else
                {
                    //frequency list
                    string freqStr = Localizer.Format("#CNC_ConstellationControl_getFreqString_nothing");//"No frequency assigned"

                    if (Frequencies.Count > 0)
                    {
                        freqStr = Localizer.Format("#CNC_CNCCommNetHome_freqlist");//"Broadcasting in"
                        for (int i = 0; i < Frequencies.Count; i++)
                            freqStr += "\n" + Localizer.Format("#CNC_CNCCommNetHome_frequency") + " " + Frequencies[i];//"~ frequency"
                    }

                    stationInfoString = string.Format("DSN Power: {1}\nTech Level: {0}\n{2}", 
                                            this.TechLevel,
                                            UIUtils.RoundToNearestMetricFactor(this.comm.antennaRelay.power, 2),
                                            freqStr);
                }

                headlineRect = groundStationRect;
                Vector2 freqDim = CNCCommNetHome.groundStationHeadline.CalcSize(new GUIContent(stationInfoString));
                headlineRect.x -= freqDim.x / 2 - 5;
                headlineRect.y += groundStationRect.height + 5;
                headlineRect.width = freqDim.x;
                headlineRect.height = freqDim.y;
                GUI.Label(headlineRect, stationInfoString, CNCCommNetHome.groundStationHeadline);
            }
        }

        /// <summary>
        /// Check whether this vector3 location is behind the body
        /// Original code by regex from https://github.com/NathanKell/RealSolarSystem/blob/master/Source/KSCSwitcher.cs
        /// </summary>
        private bool isOccluded(Vector3d position, CelestialBody body)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - position, body.position - position) > 90)
                return false;
            return true;
        }

        /// <summary>
        /// Calculate the distance between the camera position and the ground station, and
        /// return true if the distance is >= DistanceToHideGroundStations from the settings file.
        /// </summary>
        private bool IsCamDistanceToWide(Vector3d loc)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);
            float distance = Vector3.Distance(camPos, loc);

            if (distance >= CNCSettings.Instance.DistanceToHideGroundStations)
                return true;
            return false;
        }

        /// <summary>
        /// Allow to be sorted easily
        /// </summary>
        public int CompareTo(CNCCommNetHome other)
        {
            return this.stationName.CompareTo(other.stationName);
        }

        /// <summary>
        /// Regenerate frequency array used for low-gc operations
        /// </summary>
        protected void regenerateFrequencyArray(List<short> list)
        {
            if (list.Count == 0)
            {
                sorted_frequency_array = new short[] { };
            }

            sorted_frequency_array = new short[list.Count];
            for(int i=0; i< list.Count; i++)
            {
                sorted_frequency_array[i] = list[i];
            }

            GameUtils.Quicksort(sorted_frequency_array, 0, sorted_frequency_array.Length - 1);
        }

        /// <summary>
        /// Update relevant details based on Tech Level
        /// </summary>
        protected void refresh()
        {
            // Obtain Tech Level of Tracking Station in KCS
            if (this.isKSC)
            {
                this.TechLevel = (short)((2 * ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.TrackingStation)) + 1);
            }

            // Update power of ground station
            if (this.comm != null)
            {
                this.comm.antennaRelay.Update(GetDSNRange(this.TechLevel), GameVariables.Instance.GetDSNRangeCurve(), false);
            }

            // Generate ground station information
            stationInfoString = (this.TechLevel == 0) ? "Build a ground station" :
                                                    string.Format("DSN Power: {1}\nBeamwidth: {2:0.00}°\nTech Level: {0}",
                                                    this.TechLevel,
                                                    UIUtils.RoundToNearestMetricFactor(this.comm.antennaRelay.power, 2),
                                                    90.0);

            // Generate visual ground station mark
            stationTexture = CNCCommNetHome.getGroundStationTexture(this.TechLevel);

            // Update position on celestial body
            this.comm.precisePosition = this.body.GetWorldSurfacePosition(this.latitude, this.longitude, this.altitude);
        }

        /// <summary>
        /// Get ground station texture based on tech level
        /// </summary>
        public static Texture2D getGroundStationTexture(int techLevel)
        {
            switch (techLevel)
            {
                case 0:
                    return L0MarkTexture;
                case 1:
                    return L1MarkTexture;
                case 2:
                    return L2MarkTexture;
                case 3:
                default:
                    return L3MarkTexture;
            }
        }

        /// <summary>
        /// Custom DSN ranges instead of stock GameVariables.Instance.GetDSNRange
        /// </summary>
        /// Comment: Subclassing GameVariables.Instance.GetDSNRange to just change the ranges is too excessive at this point.
        public double GetDSNRange(short level)
        {
            double power;
            if (this.isKSC)
            {
                power = CNCSettings.Instance.KSCStationPowers[level - 1];
            }
            else
            {
                if (level == 0)
                {
                    power = 0;
                }
                else
                {
                    power = CNCSettings.Instance.GroundStationUpgradeablePowers[level - 1];
                }
            }

            return power * ((double)HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().DSNModifier);
        }

        /// <summary>
        /// Overrode to correct the error of assigning position to comm's position (no setter)
        /// </summary>
        protected override void Update()
        {
            if (this.comm != null)
            {
                this.comm.precisePosition = this.body.GetWorldSurfacePosition(this.lat, this.lon, this.alt);
                //this.comm.position has no setter
                this.comm.transform.position = this.comm.precisePosition;
                this.nodeTransform.position = this.comm.precisePosition;
            }
        }

        /// <summary>
        /// Overrode to remove unnecessary position calculation that is done in Update()
        /// </summary>
        protected override void OnNetworkPreUpdate()
        {
            //do nothing
        }
    }
}
