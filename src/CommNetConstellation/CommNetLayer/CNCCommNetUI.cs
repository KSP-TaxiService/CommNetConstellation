using CommNet;
using CommNetConstellation.UI;
using KSP.Localization;
using KSP.UI.Screens.Mapview;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// CommNetUI is the view in the Model–view–controller sense. Everything a player is seeing goes through this class
    /// </summary>
    public class CNCCommNetUI : CommNetUI
    {
        /// <summary>
        /// Add own display mode in replacement of stock display mode which cannot be extended easily
        /// </summary>
        public enum CustomDisplayMode
        {
            [Description("None")]
            None,
            [Description("First Hop")]
            FirstHop,
            [Description("Active Connection")]
            Path,
            [Description("Vessel Links")]
            VesselLinks,
            [Description("Network")]
            Network,
            [Description("All Active Connections")]
            MultiPaths
        }

        //New variables related to display mode
        public static CustomDisplayMode CustomMode = CustomDisplayMode.Path;
        public static CustomDisplayMode CustomModeTrackingStation = CustomDisplayMode.Network;
        public static CustomDisplayMode CustomModeFlightMap = CustomDisplayMode.Path;
        private static int CustomModeCount = Enum.GetValues(typeof(CustomDisplayMode)).Length;

        public static new CNCCommNetUI Instance
        {
            get;
            protected set;
        }

        /// <summary>
        /// Activate things when the player enter a scene that uses CommNet UI
        /// </summary>
        public override void Show()
        {
            this.lineWidth3D = this.lineWidth2D; //lineWidth3D=1 is too thin to see
            registerMapNodeIconCallbacks();
            base.Show();
        }

        /// <summary>
        /// Clean up things when the player exits a scene that uses CommNet UI
        /// </summary>
        public override void Hide()
        {
            deregisterMapNodeIconCallbacks();
            base.Hide();
        }

        /// <summary>
        /// Run own display updates
        /// </summary>
        protected override void UpdateDisplay()
        {
            if (CommNetNetwork.Instance == null)
            {
                return;
            }
            else
            {
                updateCustomisedView();
            }
        }

        /// <summary>
        /// Register own callbacks
        /// </summary>
        protected void registerMapNodeIconCallbacks()
        {
            if (CNCCommNetScenario.Instance == null)
                return;

            List<CNCCommNetVessel> commnetVessels = CNCCommNetScenario.Instance.getCommNetVessels();

            for (int i = 0; i < commnetVessels.Count; i++)
            {
                MapObject mapObj = commnetVessels[i].Vessel.mapObject;

                if (mapObj.type == MapObject.ObjectType.Vessel)
                    mapObj.uiNode.OnUpdateVisible += new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
            }
        }

        /// <summary>
        /// Remove own callbacks
        /// </summary>
        protected void deregisterMapNodeIconCallbacks()
        {
            if (CNCCommNetScenario.Instance == null)
                return;

            List<CNCCommNetVessel> commnetVessels = CNCCommNetScenario.Instance.getCommNetVessels();

            for (int i = 0; i < commnetVessels.Count; i++)
            {
                MapObject mapObj = commnetVessels[i].Vessel.mapObject;
                mapObj.uiNode.OnUpdateVisible -= new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
            }
        }

        /// <summary>
        /// Update the MapNode object of each CommNet vessel
        /// </summary>
        private void OnMapNodeUpdateVisible(MapNode node, MapNode.IconData iconData)
        {
            CNCCommNetVessel thisVessel = (CNCCommNetVessel) node.mapObject.vessel.connection;

            if(thisVessel != null && node.mapObject.type == MapObject.ObjectType.Vessel)
            {
                if (thisVessel.getStrongestFrequency() < 0) // blind vessel
                    iconData.color = Color.grey;
                else
                    iconData.color = Constellation.getColor(thisVessel.getStrongestFrequency());
            }
        }

        /// <summary>
        /// Compute the color based on the connection between two nodes
        /// </summary>
        private Color getConstellationColor(CommNode a, CommNode b)
        {
            //Assume the connection between A and B passes the check test
            short[] commonFreqs = GameUtils.NonLinqIntersect(CNCCommNetScenario.Instance.getFrequencies(a), CNCCommNetScenario.Instance.getFrequencies(b));
            short strongestFreq = -1;
            double longestRange = 0.0, thisRange;

            for (int i = 0; i < commonFreqs.Length; i++)
            {
                if (!Constellation.find(commonFreqs[i]).visibility)
                {
                    continue;
                }

                thisRange = CNCCommNetScenario.RangeModel.GetMaximumRange(CNCCommNetScenario.Instance.getCommPower(a, commonFreqs[i]), CNCCommNetScenario.Instance.getCommPower(b, commonFreqs[i]));
                if(thisRange > longestRange)
                {
                    longestRange = thisRange;
                    strongestFreq = commonFreqs[i];
                }
            }

            return Constellation.getColor(strongestFreq); 
        }

        /// <summary>
        /// Overrode ResetMode to use custom display mode
        /// </summary>
        public override void ResetMode()
        {
            CNCCommNetUI.CustomMode = CNCCommNetUI.CustomDisplayMode.None;

            if (FlightGlobals.ActiveVessel == null)
            {
                CNCCommNetUI.CustomModeTrackingStation = CNCCommNetUI.CustomMode;
            }
            else
            {
                CNCCommNetUI.CustomModeFlightMap = CNCCommNetUI.CustomMode;
            }

            this.points.Clear();
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118264", new string[]
            {
                Localizer.Format(CNCCommNetUI.CustomMode.displayDescription())
            }), CNCSettings.ScreenMessageDuration);
        }

        /// <summary>
        /// Overrode SwitchMode to use custom display mode
        /// </summary>
        public override void SwitchMode(int step)
        {
            int modeIndex = (((int)CNCCommNetUI.CustomMode) + step + CNCCommNetUI.CustomModeCount) % CNCCommNetUI.CustomModeCount;
            CNCCommNetUI.CustomDisplayMode newMode = (CNCCommNetUI.CustomDisplayMode)modeIndex;

            if (this.useTSBehavior)
            {
                this.ClampAndSetMode(ref CNCCommNetUI.CustomModeTrackingStation, newMode);
            }
            else
            {
                this.ClampAndSetMode(ref CNCCommNetUI.CustomModeFlightMap, newMode);
            }

            this.points.Clear();
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118530", new string[]
            {
                Localizer.Format(CNCCommNetUI.CustomMode.displayDescription())
            }), CNCSettings.ScreenMessageDuration);
        }

        /// <summary>
        /// Add new ClampAndSetMode for custom display mode
        /// </summary>
        public void ClampAndSetMode(ref CNCCommNetUI.CustomDisplayMode curMode, CNCCommNetUI.CustomDisplayMode newMode)
        {
            if (this.vessel == null || this.vessel.connection == null || this.vessel.connection.Comm.Net == null)
            {
                if (newMode != CNCCommNetUI.CustomDisplayMode.None &&
                    newMode != CNCCommNetUI.CustomDisplayMode.Network &&
                    newMode != CNCCommNetUI.CustomDisplayMode.MultiPaths)
                {
                    newMode = ((curMode != CNCCommNetUI.CustomDisplayMode.None) ? CNCCommNetUI.CustomDisplayMode.None : CNCCommNetUI.CustomDisplayMode.Network);
                }
            }

            CNCCommNetUI.CustomMode = (curMode = newMode);
        }

        /// <summary>
        /// Overrode UpdateDisplay() fully and add own customisations
        /// </summary>
        private void updateCustomisedView()
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                this.useTSBehavior = true;
            }
            else
            {
                this.useTSBehavior = false;
                this.vessel = FlightGlobals.ActiveVessel;
            }

            if (this.vessel == null || this.vessel.connection == null || this.vessel.connection.Comm.Net == null) //revert to default display mode if saved mode is inconsistent in current situation
            {
                this.useTSBehavior = true;
                if (CustomModeTrackingStation != CustomDisplayMode.None) 
                {
                    if (CustomModeTrackingStation != CustomDisplayMode.Network && CustomModeTrackingStation != CustomDisplayMode.MultiPaths)
                    {
                        CustomModeTrackingStation = CustomDisplayMode.Network;
                        ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118264", new string[]
                        {
                            Localizer.Format(CustomModeTrackingStation.displayDescription())
                        }), CNCSettings.ScreenMessageDuration);
                    }
                }
            }

            if (CommNetNetwork.Instance == null)
            {
                return;
            }
            if (this.useTSBehavior)
            {
                CNCCommNetUI.CustomMode = CNCCommNetUI.CustomModeTrackingStation;
            }
            else
            {
                CNCCommNetUI.CustomMode = CNCCommNetUI.CustomModeFlightMap;
            }

            CommNetwork net = CommNetNetwork.Instance.CommNet;
            CommNetVessel cnvessel = null;
            CommNode node = null;
            CommPath path = null;

            if (this.vessel != null && this.vessel.connection != null && this.vessel.connection.Comm.Net != null)
            {
                cnvessel = this.vessel.connection;
                node = cnvessel.Comm;
                path = cnvessel.ControlPath;
            }

            //work out which links to display
            int count = this.points.Count;//save previous value
            int numLinks = 0;
            bool pathLinkExist;
            switch (CNCCommNetUI.CustomMode)
            {
                case CNCCommNetUI.CustomDisplayMode.None:
                    numLinks = 0;
                    break;

                case CNCCommNetUI.CustomDisplayMode.FirstHop:
                    if (cnvessel.ControlState != VesselControlState.Probe && cnvessel.ControlState != VesselControlState.Kerbal && path != null)
                    {
                        if (path.Count != 0)
                        {
                            path.First.GetPoints(this.points);
                            numLinks = 1;
                            break;
                        }
                    }
                    numLinks = 0;
                    break;

                case CNCCommNetUI.CustomDisplayMode.Path:
                    if (cnvessel.ControlState != VesselControlState.Probe && cnvessel.ControlState != VesselControlState.Kerbal && path != null)
                    {
                        if (path.Count != 0)
                        {
                            path.GetPoints(this.points, true);
                            numLinks = path.Count;
                            break;
                        }
                    }
                    numLinks = 0;
                    break;

                case CNCCommNetUI.CustomDisplayMode.VesselLinks:
                    numLinks = node.Count;
                    node.GetLinkPoints(this.points);
                    break;

                case CNCCommNetUI.CustomDisplayMode.Network:
                    if (net.Links.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        //net.GetLinkPoints(this.points);
                        //numLinks = net.Links.Count;
                        //KSP 1.8: Likely shader changes led to stackup of 2 or more lines illustrating overall brighter line
                        //Workaround: collect unqiue lines as path
                        path = new CommPath();
                        path.Capacity = net.Links.Count;

                        for(int i=0; i<net.Links.Count; i++)
                        {
                            pathLinkExist = false;
                            for (int overallpathIndex = 0; overallpathIndex < path.Count; overallpathIndex++)//check if path has this link already
                            {
                                if (CNCCommNetwork.AreSame(path[overallpathIndex].a, net.Links[i].a) &&
                                    CNCCommNetwork.AreSame(path[overallpathIndex].b, net.Links[i].b))
                                {
                                    pathLinkExist = true;
                                    break;
                                }
                            }
                            if (!pathLinkExist)
                            {
                                path.Add(net.Links[i]);
                            }
                        }

                        path.GetPoints(this.points, true);
                        numLinks = path.Count;
                    }
                    break;

                case CNCCommNetUI.CustomDisplayMode.MultiPaths:
                    if (net.Links.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        path = new CommPath();
                        path.Capacity = net.Links.Count;

                        for(int i = 0; i < net.Count; i++)
                        {
                            var thisPath = new CommPath();
                            net.FindClosestControlSource(net[i], thisPath);
                            for (int controlpathIndex = 0; controlpathIndex < thisPath.Count; controlpathIndex++)
                            {
                                pathLinkExist = false;
                                for (int overallpathIndex = 0; overallpathIndex < path.Count; overallpathIndex++)//check if overall path has this link already
                                {
                                    if (CNCCommNetwork.AreSame(path[overallpathIndex].a, thisPath[controlpathIndex].a) &&
                                        CNCCommNetwork.AreSame(path[overallpathIndex].b, thisPath[controlpathIndex].b))
                                    {
                                        pathLinkExist = true;
                                        break;
                                    }
                                }
                                if (!pathLinkExist)
                                {
                                    path.Add(thisPath[controlpathIndex]);
                                }
                            }
                        }

                        path.GetPoints(this.points, true);
                        numLinks = path.Count;
                    }
                    break;
            }// end of switch

            //check if nothing to display
            if (numLinks == 0)
            {
                if (this.line != null)
                    this.line.active = false;

                this.points.Clear();
                return;
            }

            if (this.line != null)
            {
                this.line.active = true;
            }
            else
            {
                this.refreshLines = true;
            }

            ScaledSpace.LocalToScaledSpace(this.points); //seem very important
            if (!(!this.refreshLines && MapView.Draw3DLines == this.draw3dLines && count == this.points.Count && this.line != null))
            {
                this.CreateLine(ref this.line, this.points);//seems it is multiple separate lines not single continuous line
                this.draw3dLines = MapView.Draw3DLines;
                this.refreshLines = false;
            }

            //paint the links
            switch (CNCCommNetUI.CustomMode)
            {
                case CNCCommNetUI.CustomDisplayMode.FirstHop:
                {
                        this.line.SetColor(colorBlending(getConstellationColor(path.First.a, path.First.b), 
                                                         this.colorLow, 
                                                         Mathf.Pow((float) path.First.signalStrength, this.colorLerpPower)),
                                                         0);
                        break;
                }
                case CNCCommNetUI.CustomDisplayMode.Path:
                case CNCCommNetUI.CustomDisplayMode.MultiPaths:
                {
                        for(int i= numLinks - 1; i >= 0; i--)
                        {
                            this.line.SetColor(colorBlending(getConstellationColor(path[i].a, path[i].b),
                                                                this.colorLow,
                                                                Mathf.Pow((float) path[i].signalStrength, this.colorLerpPower)),
                                                                i);
                        }
                        break;
                }
                case CNCCommNetUI.CustomDisplayMode.VesselLinks:
                {
                        CommLink[] links = new CommLink[node.Count];
                        node.Values.CopyTo(links, 0);
                        for(int i = 0; i < links.Length; i++)
                        {
                            this.line.SetColor(colorBlending(getConstellationColor(links[i].a, links[i].b),
                                                             this.colorLow,
                                                             Mathf.Pow((float) links[i].GetSignalStrength(links[i].a != node, links[i].b != node), this.colorLerpPower)),
                                                             i);
                        }
                        break;
                }
                case CNCCommNetUI.CustomDisplayMode.Network:
                {
                        //for (int i = numLinks - 1; i >= 0; i--)
                        //{
                        //    this.line.SetColor(colorBlending(getConstellationColor(net.Links[i].a, net.Links[i].b),
                        //                                     this.colorLow,
                        //                                     Mathf.Pow((float) net.Links[i].GetBestSignal(), this.colorLerpPower)),
                        //                                     i);
                        //}
                        for (int i = numLinks - 1; i >= 0; i--)
                        {
                            this.line.SetColor(colorBlending(getConstellationColor(path[i].a, path[i].b),
                                                                this.colorLow,
                                                                Mathf.Pow((float)path[i].GetBestSignal(), this.colorLerpPower)),
                                                                i);
                        }
                        break;
                }
            } // end of switch

            if (this.draw3dLines)
            {
                this.line.SetWidth(this.lineWidth3D);
                this.line.Draw3D();
            }
            else
            {
                this.line.SetWidth(this.lineWidth2D);
                this.line.Draw();
            }
        }

        /// <summary>
        /// Compute final color based on inputs
        /// </summary>
        private Color colorBlending(Color colorHigh, Color colorLow, float colorLevel)
        {
            if (colorHigh == Color.clear)
            {
                return colorHigh;
            }
            else if (this.swapHighLow)
            {
                return Color.Lerp(colorHigh, colorLow, colorLevel);
            }
            else
            {
                return Color.Lerp(colorLow, colorHigh, colorLevel);
            }
        }
    }
}
