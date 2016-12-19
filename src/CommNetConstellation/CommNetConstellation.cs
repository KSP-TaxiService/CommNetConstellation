using UnityEngine;
using CommNet;
using CommNetConstellation.CommNetLayer;
using CommNetConstellation.UI;
using KSP.UI.Screens;

namespace CommNetConstellation
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CommNetConstellationDuplicate : CommNetConstellation { } //no futher action

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CommNetConstellation : MonoBehaviour
    {
        private ApplicationLauncherButton launcherButton = null;
        private Texture2D launcherBtnTexture;
        private ConstellationControlDialog controlDialog;

        public void Start()
        {
            CNCLog.Debug("CommNetConstellation.Start()");

            this.controlDialog = new ConstellationControlDialog("CommNet Constellation - <color=#00ff00>Control Panel</color>");
            this.launcherBtnTexture = CNCUtils.loadImage("cnclauncherbutton");

            this.launcherButton = ApplicationLauncher.Instance.AddModApplication(
                delegate { controlDialog.launch(new Object[] { }); }, controlDialog.dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.TRACKSTATION,
                launcherBtnTexture);
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

        }
    }
}
