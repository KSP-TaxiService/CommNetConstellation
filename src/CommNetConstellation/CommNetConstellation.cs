using UnityEngine;
using CommNetConstellation.UI;
using KSP.UI.Screens;

namespace CommNetConstellation
{
    /// <summary>
    /// Script to be ran in flight and tracking station
    /// </summary>
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CommNetConstellationDuplicate : CommNetConstellation
    {
        public override void Start()
        {
            this.controlDialog = new ConstellationControlDialog("CommNet Constellation - <color=#00ff00>Control Panel</color>");
            this.launcherButton = ApplicationLauncher.Instance.AddModApplication(
                delegate { controlDialog.launch(); }, controlDialog.dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.TRACKSTATION,
                UIUtils.loadImage("cnclauncherbutton"));
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CommNetConstellation : MonoBehaviour
    {
        protected ApplicationLauncherButton launcherButton = null;
        protected ConstellationControlDialog controlDialog;

        public virtual void Start()
        {
            this.controlDialog = new ConstellationControlDialog("CommNet Constellation - <color=#00ff00>Control Panel</color>");
            this.launcherButton = ApplicationLauncher.Instance.AddModApplication(
                delegate { controlDialog.launch(); }, controlDialog.dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.MAPVIEW,
                UIUtils.loadImage("cnclauncherbutton"));
        }

        public void OnDestroy()
        {
            if (launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
            }
        }
    }
}
