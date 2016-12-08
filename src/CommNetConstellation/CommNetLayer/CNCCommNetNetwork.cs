using CommNet;

/*
 Stack trace on particular method
CommNet Constellation -> debug: CNCCommNetVessel.UpdateComm()
UnityEngine.DebugLogHandler:Internal_Log(LogType, String, Object)
UnityEngine.DebugLogHandler:LogFormat(LogType, Object, String, Object[])
UnityEngine.Logger:Log(LogType, Object)
UnityEngine.Debug:LogWarning(Object)
CommNetConstellation.CNCLog:Debug(String, Object[]) (at E:\GitHub\CommNetConstellation\src\CommNetConstellation\CNCLog.cs:14)
CommNetConstellation.CommNetLayer.CNCCommNetVessel:UpdateComm() (at E:\GitHub\CommNetConstellation\src\CommNetConstellation\CommNetLayer\CNCCommNetVessel.cs:31)
CommNet.CommNetVessel:OnNetworkPreUpdate()
CommNetConstellation.CommNetLayer.CNCCommNetVessel:OnNetworkPreUpdate() (at E:\GitHub\CommNetConstellation\src\CommNetConstellation\CommNetLayer\CNCCommNetVessel.cs:26)
CommNet.CommNode:NetworkPreUpdate()
CommNet.Network.Net`4:PreUpdateNodes()
CommNet.Network.Net`4:Rebuild()
CommNet.CommNetwork:Rebuild()
CommNet.CommNetNetwork:Update()
*/

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetNetwork : CommNetNetwork
    {
        public static void upgradeToCNCCommNetNetwork()
        {
            CNCLog.Debug("CNCCommNetNetwork.upgradeToCNCCommNetNetwork()");
            CommNetNetwork.Instance = new CNCCommNetNetwork();
        }

        public CNCCommNetNetwork() : base()
        {
            base.CommNet = new CNCCommNetwork();
        }

        public override CommNetwork CommNet
        {
            get
            {
                CNCLog.Debug("CNCCommNetNetwork.CommNet - get");
                return base.CommNet;
            }

            set
            {
                CNCLog.Debug("CNCCommNetNetwork.CommNet - set");
                base.CommNet = value;
            }
        }

        public override bool GraphDirty
        {
            get
            {
                CNCLog.Debug("CNCCommNetNetwork.GraphDirty - get");
                return base.graphDirty;
            }
        }

        public override void QueueRebuild()
        {
            CNCLog.Debug("CNCCommNetNetwork.QueueRebuild()");
            base.QueueRebuild();
        }

    }
}
