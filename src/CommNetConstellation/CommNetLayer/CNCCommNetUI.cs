using CommNet;
using KSP.UI.Screens.Mapview;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Vectrosity;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetUI : CommNetUI
    {
        public static new CNCCommNetUI Instance
        {
            get;
            protected set;
        }

        protected override void UpdateDisplay()
        {
            //base.UpdateDisplay();
            updateCodes();
            for(int i=0; i< CNCCommNetScenario.Instance.constellations.Count; i++)
            {
                Constellation thisConstellation = CNCCommNetScenario.Instance.constellations[i];
                coloriseEachConstellation(thisConstellation.frequency, thisConstellation.color);
            }
        }

        private void coloriseEachConstellation(short radioFrequency, Color newColor)
        {
            List<CNCCommNetVessel> commnetVessels = CNCUtils.getCommNetVessels(radioFrequency);

            for (int i = 0; i < commnetVessels.Count; i++)
            {
                MapObject mapObj = commnetVessels[i].Vessel.mapObject;

                if (mapObj.type == MapObject.ObjectType.Vessel)
                {
                    Image thisImageIcon = mapObj.uiNode.GetComponentInChildren<Image>();
                    thisImageIcon.color = newColor;
                }
            }
        }

        private Color getConstellationColor(CommNode a, CommNode b)
        {
            if (a.isHome || b.isHome)
                return Constellation.getColor(CNCSettings.Instance.PublicRadioFrequency); // public

            CNCCommNetVessel vesselA = (CNCCommNetVessel)CNCUtils.findCorrespondingVessel(a).Connection;
            CNCCommNetVessel vesselB = (CNCCommNetVessel)CNCUtils.findCorrespondingVessel(b).Connection;

            if(vesselA.getRadioFrequency() == vesselB.getRadioFrequency())
                return Constellation.getColor(vesselA.getRadioFrequency());
            else
                return Constellation.getColor(CNCSettings.Instance.PublicRadioFrequency); // public
        }

        private void updateCodes() // TODO: try to remove most of recycled stock codes
        {
            if (FlightGlobals.fetch.activeVessel == null)
            {
                this.useTSBehavior = true;
            }
            else
            {
                this.useTSBehavior = false;
                this.vessel = FlightGlobals.fetch.activeVessel;
            }
            if (this.vessel == null || this.vessel.connection == null || this.vessel.connection.Comm.Net == null)
            {
                this.useTSBehavior = true;
                if (CommNetUI.ModeTrackingStation != CommNetUI.DisplayMode.None)
                {
                    if (CommNetUI.ModeTrackingStation != CommNetUI.DisplayMode.Network)
                    {
                        ScreenMessages.PostScreenMessage("CommNet Visualization set to " + CommNetUI.DisplayMode.Network.ToString(), 5f);
                    }
                    CommNetUI.ModeTrackingStation = CommNetUI.DisplayMode.Network;
                }
            }
            if (CommNetNetwork.Instance == null)
            {
                return;
            }
            if (this.useTSBehavior)
            {
                CommNetUI.Mode = CommNetUI.ModeTrackingStation;
            }
            else
            {
                CommNetUI.Mode = CommNetUI.ModeFlightMap;
            }
            CommNetwork commNet = CommNetNetwork.Instance.CommNet;
            CommNetVessel commNetVessel = null;
            CommNode commNode = null;
            CommPath commPath = null;
            if (this.vessel != null && this.vessel.connection != null && this.vessel.connection.Comm.Net != null)
            {
                commNetVessel = this.vessel.connection;
                commNode = commNetVessel.Comm;
                commPath = commNetVessel.ControlPath;
            }
            int count = this.points.Count;
            int num = 0;
            switch (CommNetUI.Mode)
            {
                case CommNetUI.DisplayMode.None:
                    num = 0;
                    break;
                case CommNetUI.DisplayMode.FirstHop:
                    if (commNetVessel.ControlState == VesselControlState.Probe || commNetVessel.ControlState == VesselControlState.Kerbal || commPath == null || commPath.Count == 0)
                    {
                        num = 0;
                    }
                    else
                    {
                        commPath.First.GetPoints(this.points);
                        num = 1;
                    }
                    break;
                case CommNetUI.DisplayMode.Path:
                    if (commNetVessel.ControlState == VesselControlState.Probe || commNetVessel.ControlState == VesselControlState.Kerbal || commPath == null || commPath.Count == 0)
                    {
                        num = 0;
                    }
                    else
                    {
                        commPath.GetPoints(this.points, true);
                        num = commPath.Count;
                    }
                    break;
                case CommNetUI.DisplayMode.VesselLinks:
                    num = commNode.Count;
                    commNode.GetLinkPoints(this.points);
                    break;
                case CommNetUI.DisplayMode.Network:
                    if (this.vessel == null)
                    {
                    }
                    if (commNet.Links.Count == 0)
                    {
                        num = 0;
                    }
                    else
                    {
                        commNet.GetLinkPoints(this.points);
                        num = commNet.Links.Count;
                    }
                    break;
            }
            if (num == 0)
            {
                if (this.line != null)
                {
                    this.line.active = false;
                }
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
            ScaledSpace.LocalToScaledSpace(this.points);
            if (this.refreshLines || MapView.Draw3DLines != this.draw3dLines || count != this.points.Count || this.line == null)
            {
                this.CreateLine(ref this.line, this.points);
                this.draw3dLines = MapView.Draw3DLines;
                this.refreshLines = false;
            }
            switch (CommNetUI.Mode)
            {
                case CommNetUI.DisplayMode.FirstHop:
                    {
                        float f = (float)commPath.First.signalStrength;
                        float t = Mathf.Pow(f, this.colorLerpPower);
                        Color customHighColor = getConstellationColor(commPath.First.a, commPath.First.b);
                        if (this.swapHighLow)
                        {
                            this.line.SetColor(Color.Lerp(customHighColor, this.colorLow, t), 0);
                        }
                        else
                        {
                            this.line.SetColor(Color.Lerp(this.colorLow, customHighColor, t), 0);
                        }
                        break;
                    }
                case CommNetUI.DisplayMode.Path:
                    {
                        int index = num;
                        while (index-- > 0)
                        {
                            float f = (float)commPath[index].signalStrength;
                            float t = Mathf.Pow(f, this.colorLerpPower);
                            Color customHighColor = getConstellationColor(commPath[index].a, commPath[index].b);
                            if (this.swapHighLow)
                            {
                                this.line.SetColor(Color.Lerp(customHighColor, this.colorLow, t), index);
                            }
                            else
                            {
                                this.line.SetColor(Color.Lerp(this.colorLow, customHighColor, t), index);
                            }
                        }
                        break;
                    }
                case CommNetUI.DisplayMode.VesselLinks:
                    {
                        Dictionary<CommNode, CommLink>.ValueCollection.Enumerator enumerator = commNode.Values.GetEnumerator();
                        int num2 = 0;
                        while (enumerator.MoveNext())
                        {
                            CommLink commLink = enumerator.Current;
                            float f = (float)commLink.GetSignalStrength(commLink.a != commNode, commLink.b != commNode);
                            float t = Mathf.Pow(f, this.colorLerpPower);
                            Color customHighColor = getConstellationColor(commLink.a, commLink.b);
                            if (this.swapHighLow)
                            {
                                this.line.SetColor(Color.Lerp(customHighColor, this.colorLow, t), num2);
                            }
                            else
                            {
                                this.line.SetColor(Color.Lerp(this.colorLow, customHighColor, t), num2);
                            }
                            num2++;
                        }
                        break;
                    }
                case CommNetUI.DisplayMode.Network:
                    {
                        int index2 = num;
                        while (index2-- > 0)
                        {
                            CommLink commLink = commNet.Links[index2];
                            float f = (float)commNet.Links[index2].GetBestSignal();
                            float t = Mathf.Pow(f, this.colorLerpPower);
                            Color customHighColor = getConstellationColor(commLink.a, commLink.b);
                            if (this.swapHighLow)
                            {
                                this.line.SetColor(Color.Lerp(customHighColor, this.colorLow, t), index2);
                            }
                            else
                            {
                                this.line.SetColor(Color.Lerp(this.colorLow, customHighColor, t), index2);
                            }
                        }
                        break;
                    }
            }
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
    }
}
