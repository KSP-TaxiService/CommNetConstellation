using System.Collections.Generic;
using System.Linq;

using CommNetConstellation.CommNetLayer;
using CommNet;

namespace CommNetConstellation
{
    public class CNCUtils
    {
        public static List<CNCCommNetVessel> getCommNetVessels(short targetRadioFrequency = -1)
        {
            List<Vessel> vessels = FlightGlobals.Vessels;
            List<CNCCommNetVessel> commnetVessels = new List<CNCCommNetVessel>();

            for (int i=0; i<vessels.Count; i++)
            {
                Vessel thisVessel = vessels[i];
                if(thisVessel.Connection != null)
                {
                    CNCCommNetVessel cncVessel = (CNCCommNetVessel)thisVessel.Connection;
                    if(cncVessel.getRadioFrequency() == targetRadioFrequency || targetRadioFrequency == -1)
                        commnetVessels.Add(cncVessel);
                }
            }

            return commnetVessels;
        }

        public static Vessel findCorrespondingVessel(CommNode commNodeRef)
        {
            List<Vessel> allVessels = FlightGlobals.Vessels;
            IEqualityComparer<CommNode> comparer = commNodeRef.Comparer;

            //brute-force search temporarily until I find a \omega(n) method
            for (int i = 0; i < allVessels.Count(); i++)
            {
                Vessel thisVessel = allVessels[i];
                if (thisVessel.connection != null)
                {
                    if (comparer.Equals(commNodeRef, thisVessel.connection.Comm))
                        return thisVessel;
                }
            }

            //not found
            return null;
        }
    }
}
