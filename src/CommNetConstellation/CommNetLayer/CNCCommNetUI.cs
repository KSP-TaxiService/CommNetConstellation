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
            base.colorHigh = new Color(0.43f, 0.81f, 0.96f, 1f); // blue
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void UpdateDisplay()
        {
            base.UpdateDisplay();

            List<CNCCommNetVessel> commnetVessels = CNCUtils.getCommNetVessels(CNCSettings.Instance.PublicRadioFrequency);

            for(int i=0; i<commnetVessels.Count; i++)
            {
                MapObject mapObj = commnetVessels.ElementAt(i).Vessel.mapObject;
                
                if(mapObj.type == MapObject.ObjectType.Vessel)
                {
                    Image thisImageIcon = mapObj.uiNode.GetComponentInChildren<Image>();
                    thisImageIcon.color = new Color(0.43f, 0.81f, 0.96f, 1f);
                    CNCLog.Debug("Vessel '{0}' has radio frequency {1}", commnetVessels.ElementAt(i).Vessel.GetName(), commnetVessels.ElementAt(i).getRadioFrequency());
                }
            }      
        }


    }
}
