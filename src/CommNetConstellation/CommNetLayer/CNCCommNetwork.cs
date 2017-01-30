using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetwork : CommNetwork
    {
        private short publicFreq = CNCSettings.Instance.PublicRadioFrequency;

        protected override bool SetNodeConnection(CommNode a, CommNode b)
        {
            short aFreq = (a.isHome)?publicFreq : ((CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(a).Connection).getRadioFrequency();
            short bFreq = (b.isHome)?publicFreq : ((CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(b).Connection).getRadioFrequency();

            if (aFreq != bFreq && aFreq != publicFreq && bFreq != publicFreq)
            {
                this.Disconnect(a, b, true);
                return false;
            }

            return base.SetNodeConnection(a, b);
        }
    }
}
