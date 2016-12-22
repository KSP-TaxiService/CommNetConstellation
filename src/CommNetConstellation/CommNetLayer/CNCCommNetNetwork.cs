using CommNet;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetNetwork : CommNetNetwork
    {
        //protected CNCCommNetwork commNet;

        public CNCCommNetNetwork()
        {
            CNCLog.Debug("CNCCommNetNetwork()");
        }

        public static new CNCCommNetNetwork Instance
        {
            get;
            protected set;
        }

        public override CommNetwork CommNet
        {
            get
            {
                return this.commNet as CNCCommNetwork;
            }
            set
            {
                this.commNet = value;
            }
        }

        protected override void Awake()
        {
            if (CNCCommNetNetwork.Instance != null && CNCCommNetNetwork.Instance != this)
            {
                UnityEngine.Object.DestroyImmediate(CNCCommNetNetwork.Instance);
            }
            CNCCommNetNetwork.Instance = this;
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                GameEvents.onPlanetariumTargetChanged.Add(new EventData<MapObject>.OnEvent(this.OnMapFocusChange));
            }
            GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(this.ResetNetwork));
            CNCCommNetNetwork.Reset();
        }

        protected override void OnDestroy()
        {
            if (CNCCommNetNetwork.Instance != null && CNCCommNetNetwork.Instance == this)
            {
                CNCCommNetNetwork.Instance = null;
            }
            GameEvents.onPlanetariumTargetChanged.Remove(new EventData<MapObject>.OnEvent(this.OnMapFocusChange));
            GameEvents.OnGameSettingsApplied.Remove(new EventVoid.OnEvent(this.ResetNetwork));
        }

        /*
        bool runOnce = false;
        protected override void Update()
        {
            base.Update();
            if (!runOnce)
            {
                this.DebugInfo();
                runOnce = true;
            }
        }
        */

        protected override void OnMapFocusChange(MapObject target)
        {
            CNCLog.Debug("CNCCommNetNetwork.OnMapFocusChange() : {0}", target.name);
            base.OnMapFocusChange(target);
        }

        protected new void ResetNetwork()
        {
            CNCLog.Debug("CNCCommNetNetwork.ResetNetwork()");
            this.CommNet = new CNCCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
        }
        
        public static new void Reset()
        {
            if (CNCCommNetNetwork.Instance != null)
            {
                CNCCommNetNetwork.Instance.ResetNetwork();
            }
        }

        public static new void Add(CommNode node)
        {
            if (CNCCommNetNetwork.Instance != null && CNCCommNetNetwork.Instance.commNet != null)
            {
                CNCCommNetNetwork.Instance.commNet.Add(node);
            }
        }

        public static new void Remove(CommNode node)
        {
            if (CNCCommNetNetwork.Instance != null && CNCCommNetNetwork.Instance.commNet != null)
            {
                CNCCommNetNetwork.Instance.commNet.Remove(node);
            }
        }

        public static new void Add(Occluder occluder)
        {
            if (CNCCommNetNetwork.Instance != null && CNCCommNetNetwork.Instance.commNet != null)
            {
                CNCCommNetNetwork.Instance.commNet.Add(occluder);
            }
        }

        public static new void Remove(Occluder occluder)
        {
            if (CNCCommNetNetwork.Instance != null && CNCCommNetNetwork.Instance.commNet != null)
            {
                CNCCommNetNetwork.Instance.commNet.Remove(occluder);
            }
        }
    }
}
