using CommNet;
using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using UnityEngine;

namespace CommNetConstellation.UI.VesselMgtTools
{
    public abstract class AbstractMgtTool
    {
        protected List<short> initialFrequencies;
        protected CNCCommNetVessel cncVessel;
        protected List<CNCAntennaPartInfo> antennas;
        protected Callback updateCallback;

        protected string codename;
        public string toolName;
        protected UIStyle style;

        public AbstractMgtTool(CommNetVessel thisVessel, string uniqueCodename, string toolName, Callback updateCallback = null)
        {
            if (!(thisVessel is CNCCommNetVessel))
                CNCLog.Error("Vessel '{0}''s connection is not of type CNCCommNetVessel!", thisVessel.Vessel.vesselName);

            this.cncVessel = (CNCCommNetVessel)thisVessel;
            this.initialFrequencies = this.cncVessel.getFrequencies();
            this.antennas = this.cncVessel.getAllAntennaInfo();

            this.codename = uniqueCodename+"_mgttool";
            this.toolName = toolName;
            this.updateCallback = updateCallback;

            style = new UIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Normal;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;
        }

        public abstract List<DialogGUIBase> getContentComponents();
        public abstract void run();//any use case?

        public virtual void prerun()//any use case?
        {
            //do nothing
        }
    }
}
