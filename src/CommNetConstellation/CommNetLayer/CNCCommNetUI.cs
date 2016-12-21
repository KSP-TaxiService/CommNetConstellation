using CommNet;
using KSP.UI.Screens.Mapview;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetUI : CommNetUI
    {
        public CNCCommNetUI()
        {
            //base.colorHigh = new Color(0.43f, 0.81f, 0.96f, 1f); // blue
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void UpdateDisplay()
        {
            base.UpdateDisplay();
            coloriseEachConstellation(CNCSettings.Instance.PublicRadioFrequency, new Color(0.65f, 0.65f, 0.65f, 1f));
            coloriseEachConstellation(1, new Color(0.43f, 0.81f, 0.96f, 1f));
            coloriseEachConstellation(2, new Color(0.95f, 0.43f, 0.49f, 1f));
        }

        private void coloriseEachConstellation(int radioFrequency, Color newColor)
        {
            List<CNCCommNetVessel> commnetVessels = CNCUtils.getCommNetVessels(radioFrequency);

            for (int i = 0; i < commnetVessels.Count; i++)
            {
                MapObject mapObj = commnetVessels.ElementAt(i).Vessel.mapObject;

                if (mapObj.type == MapObject.ObjectType.Vessel)
                {
                    Image thisImageIcon = mapObj.uiNode.GetComponentInChildren<Image>();
                    thisImageIcon.color = newColor;
                }
            }

            //TODO: color links
            //line.SetColor(newColor);
            //line.Draw();

            /*
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

            switch (CommNetUI.Mode)
            {
                case CommNetUI.DisplayMode.Network:
                {
                    int index2 = num;
                    while (index2-- > 0)
                    {
                        CommLink commLink = commNet.Links[index2];
                        float f = (float)commNet.Links[index2].GetBestSignal();
                        float t = Mathf.Pow(f, this.colorLerpPower);
                        if (this.swapHighLow)
                        {
                            this.line.SetColor(Color.Lerp(this.colorHigh, this.colorLow, t), index2);
                        }
                        else
                        {
                            this.line.SetColor(Color.Lerp(this.colorLow, this.colorHigh, t), index2);
                        }
                    }
                    break;
                }
            }
            */
        }
    }
}
