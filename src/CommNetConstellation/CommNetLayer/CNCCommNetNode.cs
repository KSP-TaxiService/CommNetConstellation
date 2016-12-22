using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    ///     ILSpy indicates this class is never used by KSP or CommNet in any way. Maybe the unused class?
    /// </summary>

    public class CNCCommNetNode : CommNetNode
    {
        public void copyOf(CommNetNode stockNode)
        {
            this.Comm = stockNode.Comm;
            this.networkInitialised = stockNode.GetComponentInChildren<bool>();
        }

        protected override void Start()
        {
            CNCLog.Debug("CNCCommNetNode.Start() : NOTE - No CommNetNode object found in CommNet");
            base.Start();
            if (CommNetNetwork.Initialized)
            {
                CNCCommNetNetwork.Add(this.comm);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!this.networkInitialised)
            {
                CNCCommNetNetwork.Remove(this.comm);
            }
        }

        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            CNCCommNetNetwork.Add(this.comm);
        }

    }
}
