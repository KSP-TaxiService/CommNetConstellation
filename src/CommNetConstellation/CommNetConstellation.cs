using UnityEngine;
using CommNet;
using CommNetConstellation.CommNetLayer;
using CommNetConstellation.UI;
using KSP.UI.Screens;

namespace CommNetConstellation
{
    // Called when you are in the flight scene
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CommNetConstellation : MonoBehaviour
    {
        private ApplicationLauncherButton launcherButton = null;
        private Texture2D launcherBtnTexture;
        private ConstellationControlDialog controlDialog;

        public void Start()
        {
            CNCLog.Debug("CommNetConstellation.Start()");

            this.controlDialog = new ConstellationControlDialog("CommNet Constellation", "Control panel for managing multiple constellations of satellites");
            this.launcherBtnTexture = CNCUtils.loadImage("cnclauncherbutton");

            this.launcherButton = ApplicationLauncher.Instance.AddModApplication(
                controlDialog.launch, controlDialog.dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.TRACKSTATION,
                launcherBtnTexture);

            //if (!CommNetNetwork.Initialized)
            //CNCCommNetNetwork.upgradeToCNCCommNetNetwork();
        }

        public void OnDestroy()
        {
            if (launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
            }
        }

        public void Update()
        {
            return;
            if (CommNetNetwork.Initialized)
            {
                if (CommNetNetwork.Instance.CommNet is CommNetwork)
                    CNCLog.Debug("CommNetConstellation.Update() : CommNetwork type");

                if (CommNetNetwork.Instance.CommNet is CNCCommNetwork)
                    CNCLog.Debug("CommNetConstellation.Update() : CNCCommNetwork type");
            }
        }
    }
}
