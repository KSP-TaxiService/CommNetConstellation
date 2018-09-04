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
            SetupAppLauncher(ApplicationLauncher.AppScenes.TRACKSTATION);
        }

        protected override void Launch()
        {
            if (this.controlDialog == null)
            {
                this.controlDialog = new ConstellationControlDialog("CommNet Constellation - <color=#00ff00>Control Panel</color>");
            }
            this.controlDialog.launch();
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CommNetConstellation : MonoBehaviour
    {
        protected ApplicationLauncherButton launcherButton = null;
        protected ConstellationControlDialog controlDialog;

        public virtual void Start()
        {
            SetupAppLauncher(ApplicationLauncher.AppScenes.MAPVIEW);
        }

        public void OnDestroy()
        {
            if (launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
            }
        }

        protected virtual void Launch()
        {
            if (this.controlDialog == null)
            {
                this.controlDialog = new ConstellationControlDialog("CommNet Constellation - <color=#00ff00>Control Panel</color>");
            }
            this.controlDialog.launch();
        }

        protected virtual void Dismiss()
        {
            if (this.controlDialog != null)
            {
                this.controlDialog.dismiss();
                this.controlDialog = null;
            }
        }

        protected virtual void SetupAppLauncher(ApplicationLauncher.AppScenes scenes)
        {
            this.launcherButton = ApplicationLauncher.Instance.AddModApplication(
                Launch, Dismiss, OnHover, OnHoverOut, OnEnable, OnDisable, 
                scenes, UIUtils.loadImage("cnclauncherbutton"));
        }

        /// <summary>
        /// Called when scene is entered
        /// </summary>
        protected virtual void OnEnable()
        {
        }

        /// <summary>
        /// Called when scene is exited
        /// </summary>
        protected virtual void OnDisable()
        {
        }

        /// <summary>
        /// Called when mouse cursor is over app button
        /// </summary>
        protected virtual void OnHover()
        {
        }

        /// <summary>
        /// Called when mouse cursor is out of app button
        /// </summary>
        protected virtual void OnHoverOut()
        {
        }
    }
}
