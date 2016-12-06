using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNode : CommNode
    {
        public CNCCommNode()
        {

        }

        //Copy constructor
        public CNCCommNode(CommNode stockCommNode)
        {
            this.antennaRelay = stockCommNode.antennaRelay;
            this.antennaTransmit = stockCommNode.antennaTransmit;
            this.distanceOffset = stockCommNode.distanceOffset;
            this.isControlSource = stockCommNode.isControlSource;
            this.isControlSourceMultiHop = stockCommNode.isControlSourceMultiHop;
            this.isHome = stockCommNode.isHome;
            this.OnLinkCreateSignalModifier = stockCommNode.OnLinkCreateSignalModifier;
            this.OnNetworkPostUpdate = stockCommNode.OnNetworkPostUpdate;
            this.OnNetworkPreUpdate = stockCommNode.OnNetworkPreUpdate;
            this.scienceCurve = stockCommNode.scienceCurve;
            this._name = stockCommNode.name;
            this._position = stockCommNode.position;
        }
        
        public override void NetworkPreUpdate()
        {
            CNCLog.Debug("CNCCommNode.NetworkPreUpdate()");
            base.NetworkPreUpdate();
        }

        public override void NetworkPostUpdate()
        {
            CNCLog.Debug("CNCCommNode.NetworkPostUpdate()");
            base.NetworkPostUpdate();
        }
        
    }
}
