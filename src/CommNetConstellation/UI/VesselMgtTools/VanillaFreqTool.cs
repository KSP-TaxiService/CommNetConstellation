using System;
using System.Collections.Generic;
using CommNet;
using UnityEngine;

namespace CommNetConstellation.UI.VesselMgtTools
{
    public class VanillaFreqTool : AbstractMgtTool
    {
        public VanillaFreqTool(CommNetVessel thisVessel) : base(thisVessel, "vanilla", "Vanilla")
        {
        }

        public override List<DialogGUIBase> getContentComponents()
        {
            List<DialogGUIBase> layout = new List<DialogGUIBase>();

            DialogGUILabel msgLbl = new DialogGUILabel("Original functionality of setting a single frequency of this vessel\n");
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            return layout;
        }

        public override void run()
        {
            throw new NotImplementedException();
        }
    }
}
