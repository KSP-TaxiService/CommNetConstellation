using CommNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// CommNetUI is the view in the Model–view–controller sense. Everything a player is seeing goes through this class
    /// </summary>
    public class CNCCommNetUI : CommNetUI
    {
        public static new CNCCommNetUI Instance
        {
            get;
            protected set;
        }

        /// <summary>
        /// This is the method where the interface is updated throughly
        /// </summary>
        protected override void UpdateDisplay()
        {
            base.UpdateDisplay();
            updateView();
            for(int i=0; i< CNCCommNetScenario.Instance.constellations.Count; i++)
            {
                Constellation thisConstellation = CNCCommNetScenario.Instance.constellations[i];
                coloriseConstellationMember(thisConstellation.frequency, thisConstellation.color);
            }
        }

        /// <summary>
        /// Paint each member of the given constellation
        /// </summary>
        private void coloriseConstellationMember(short radioFrequency, Color newColor)
        {
            List<CNCCommNetVessel> commnetVessels = CNCCommNetScenario.Instance.getCommNetVessels(radioFrequency);

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

        /// <summary>
        /// Compute the color based on the connection between two nodes
        /// </summary>
        private Color getConstellationColor(CommNode a, CommNode b)
        {
            if (a.isHome || b.isHome)
                return Constellation.getColor(CNCSettings.Instance.PublicRadioFrequency); // public

            CNCCommNetVessel vesselA = (CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(a).Connection;
            CNCCommNetVessel vesselB = (CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(b).Connection;

            if(vesselA.getRadioFrequency() == vesselB.getRadioFrequency())
                return Constellation.getColor(vesselA.getRadioFrequency());
            else
                return Constellation.getColor(CNCSettings.Instance.PublicRadioFrequency); // public
        }

        /// <summary>
        /// Contain relevant codes of stock UpdateDisplay() with few changes
        /// </summary>
        private void updateView()
        {
            int numLinks = 0;
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

            //work out how many connections to paint
            switch (CommNetUI.Mode)
            {
                case CommNetUI.DisplayMode.None:
                    numLinks = 0;
                    break;
                case CommNetUI.DisplayMode.FirstHop:
                    if (commNetVessel.ControlState == VesselControlState.Probe || commNetVessel.ControlState == VesselControlState.Kerbal || commPath == null || commPath.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        commPath.First.GetPoints(this.points);
                        numLinks = 1;
                    }
                    break;
                case CommNetUI.DisplayMode.Path:
                    if (commNetVessel.ControlState == VesselControlState.Probe || commNetVessel.ControlState == VesselControlState.Kerbal || commPath == null || commPath.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        commPath.GetPoints(this.points, true);
                        numLinks = commPath.Count;
                    }
                    break;
                case CommNetUI.DisplayMode.VesselLinks:
                    numLinks = commNode.Count;
                    commNode.GetLinkPoints(this.points);
                    break;
                case CommNetUI.DisplayMode.Network:
                    if (commNet.Links.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        commNet.GetLinkPoints(this.points);
                        numLinks = commNet.Links.Count;
                    }
                    break;
            }// end of switch

            //paint eligible connections
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
                    int index = numLinks;
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
                    int index2 = numLinks;
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
            } // end of switch
        }
    }
}
