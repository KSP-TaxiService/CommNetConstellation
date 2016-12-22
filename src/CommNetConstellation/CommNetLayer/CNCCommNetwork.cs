using System;
using CommNet;
using CommNet.Network;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetwork : CommNetwork
    {
        private short publicFreq = CNCSettings.Instance.PublicRadioFrequency;

        protected override bool SetNodeConnection(CommNode a, CommNode b)
        {
            if (a.isHome && b.isHome)
            {
                this.Disconnect(a, b, true);
                return false;
            }
            if (a.antennaRelay.power + a.antennaTransmit.power == 0.0 || b.antennaRelay.power + b.antennaTransmit.power == 0.0)
            {
                this.Disconnect(a, b, true);
                return false;
            }

            //My own Constellation rules
            //----------
            short aFreq = publicFreq;
            short bFreq = publicFreq;

            if (!a.isHome)
            {
                aFreq = ((CNCCommNetVessel)CNCUtils.findCorrespondingVessel(a).Connection).getRadioFrequency();
            }
            if (!b.isHome)
            {
                bFreq = ((CNCCommNetVessel)CNCUtils.findCorrespondingVessel(b).Connection).getRadioFrequency();
            }

            if (aFreq != bFreq && aFreq != publicFreq && bFreq != publicFreq)
            {
                this.Disconnect(a, b, true);
                return false;
            }
            //----------

            Vector3d precisePosition = a.precisePosition;
            Vector3d precisePosition2 = b.precisePosition;
            double num = (precisePosition2 - precisePosition).sqrMagnitude;
            double num2 = a.distanceOffset + b.distanceOffset;
            if (num2 != 0.0)
            {
                num2 = Math.Sqrt(num) + num2;
                if (num2 > 0.0)
                {
                    num = num2 * num2;
                }
                else
                {
                    num2 = (num = 0.0);
                }
            }
            bool flag = CommNetScenario.RangeModel.InRange(a.antennaRelay.power, b.antennaRelay.power, num);
            bool flag2 = flag;
            bool flag3 = flag;
            if (!flag)
            {
                flag2 = CommNetScenario.RangeModel.InRange(a.antennaRelay.power, b.antennaTransmit.power, num);
                flag3 = CommNetScenario.RangeModel.InRange(a.antennaTransmit.power, b.antennaRelay.power, num);
            }
            if (!flag2 && !flag3)
            {
                this.Disconnect(a, b, true);
                return false;
            }
            if (num == 0.0 && (flag || flag2 || flag3))
            {
                return this.TryConnect(a, b, 1E-07, flag2, flag3, flag);
            }
            if (num2 == 0.0)
            {
                num2 = Math.Sqrt(num);
            }
            if (this.TestOcclusion(precisePosition, a.occluder, precisePosition2, b.occluder, num2))
            {
                return this.TryConnect(a, b, num2, flag2, flag3, flag);
            }
            this.Disconnect(a, b, true);
            return false;
        }

        public override CommNode Add(CommNode conn)
        {
            CNCLog.Debug("CNCCommNetwork.Add() -  CommNode");
            return base.Add(conn);
        }

        public override Occluder Add(Occluder conn)
        {
            CNCLog.Debug("CNCCommNetwork.Add() - Occluder");
            return base.Add(conn);
        }
    }
}
