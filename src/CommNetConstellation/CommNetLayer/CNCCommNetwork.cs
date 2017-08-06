using CommNet;
using System;
using CommNetManagerAPI;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Extend the functionality of the KSP's CommNetwork (secondary model in the Model–view–controller sense)
    /// </summary>
    public class CNCCommNetwork : CommNetwork
    {
        private short publicFreq = CNCSettings.Instance.PublicRadioFrequency;

        /// <summary>
        /// Edit the connectivity between two potential nodes
        /// </summary>
        [CNMAttrAndOr(CNMAttrAndOr.options.AND)]
        [CNMAttrSequence(CNMAttrSequence.options.LATE)]
        protected override bool SetNodeConnection(CommNode a, CommNode b)
        {
            short aFreq, bFreq;

            try
            {
                aFreq = (a.isHome) ? publicFreq : ((ModularCommNetVessel)a.GetVessel().Connection).GetModuleOfType<CNCCommNetVessel>().getRadioFrequency();//((CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(a).Connection).getRadioFrequency();
                bFreq = (b.isHome) ? publicFreq : ((ModularCommNetVessel)b.GetVessel().Connection).GetModuleOfType<CNCCommNetVessel>().getRadioFrequency();//((CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(b).Connection).getRadioFrequency();
            }
            catch(NullReferenceException e) // either CommNode could be a kerbal on EVA
            {
                this.Disconnect(a, b, true);
                return false;
            }

            if (aFreq != bFreq && aFreq != publicFreq && bFreq != publicFreq) //check if two nodes talk, using same non-public frequency
            {
                this.Disconnect(a, b, true);
                return false;
            }

            bool aMembershipFlag, bMembershipFlag;

            try
            {
                aMembershipFlag = (a.isHome) ? false : ((ModularCommNetVessel)a.GetVessel().Connection).GetModuleOfType<CNCCommNetVessel>().getMembershipFlag();
                bMembershipFlag = (b.isHome) ? false : ((ModularCommNetVessel)b.GetVessel().Connection).GetModuleOfType<CNCCommNetVessel>().getMembershipFlag();
            }
            catch(NullReferenceException e) // either CommNode could be a kerbal on EVA
            {
                this.Disconnect(a, b, true);
                return false;
            }

            if ((aMembershipFlag && aFreq != bFreq && aFreq != publicFreq) ||
                (bMembershipFlag && aFreq != bFreq && bFreq != publicFreq)) // check if either node has membership flag to talk to members only
            {
                this.Disconnect(a, b, true);
                return false;
            }

            return CommNetManagerChecker.CommNetManagerInstalled ? true : base.SetNodeConnection(a, b);
        }
    }
}
