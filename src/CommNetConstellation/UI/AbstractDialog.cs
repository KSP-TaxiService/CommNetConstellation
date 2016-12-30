using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

/* References
 * http://forum.kerbalspaceprogram.com/index.php?/topic/149324-popupdialog-and-the-dialoggui-classes/
 * http://forum.kerbalspaceprogram.com/index.php?/topic/151354-unity-ui-creation-tutorial/
 * http://forum.kerbalspaceprogram.com/index.php?/topic/151270-gui-scrolling-problem/
 */

namespace CommNetConstellation.UI
{
    //TODO: change extraArgs to enum array
    //TODO: close button need callback (or a new method ExecuteStuffAtClosing()?)
    //TODO: resolve the issue of images being larger than 32x32

    public abstract class AbstractDialog
    {
        protected bool isDisplayed = false;
        protected string dialogTitle;
        protected int windowWidth;
        protected int windowHeight;
        protected float normalizedCenterX; //0.0f to 1.0f
        protected float normalizedCenterY; //0.0f to 1.0f
        protected bool showCloseButton = true;
        protected bool showVersion = false;
        protected bool blockBackgroundInputs = true;

        protected PopupDialog popupDialog = null;

        public AbstractDialog(string dialogTitle, float normalizedCenterX, float normalizedCenterY, int windowWidth, int windowHeight, string[] extraArgs)
        {
            this.dialogTitle = dialogTitle;
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
            this.normalizedCenterX = normalizedCenterX;
            this.normalizedCenterY = normalizedCenterY;

            processArguments(extraArgs);
        }

        protected abstract bool runIntenseInfo(System.Object[] args);
        protected abstract List<DialogGUIBase> drawContentComponents();
        protected virtual void OnUpdate() { }
        protected virtual void OnResize() { }

        public void launch(System.Object[] args)
        {
            if (this.isDisplayed)
                return;

            this.isDisplayed = true;
            if (runIntenseInfo(args))
                popupDialog = spawnDialog();
        }

        public void dismiss()
        {
            if (this.isDisplayed && popupDialog != null)
            {
                popupDialog.Dismiss();
                this.isDisplayed = false;
            }
        }

        protected virtual void processArguments(string[] args)
        {
            if (args == null)
                return;

            for(int i=0; i<args.Length; i++)
            {
                string arg = args[i];
                if (arg.Equals("hideclosebutton"))
                    this.showCloseButton = false;
                else if (arg.Equals("showversion"))
                    this.showVersion = true;
                else if (arg.Equals("allowbginputs"))
                    this.blockBackgroundInputs = false;
                else
                    CNCLog.Error("AbstractDialog argument '{0}' is unknown", arg);
            }
        }

        private PopupDialog spawnDialog()
        {
            /* This dialog looks like below
             * -----------------------
             * |        TITLE        |
             * |---------------------|
             * |                     |
             * |       CONTENT       |
             * |                     |
             * |---------------------|
             * |       [CLOSE]   [XX]|
             * ----------------------- 
             */

            List<DialogGUIBase> dialogComponentList;

            //content
            List<DialogGUIBase> contentComponentList = drawContentComponents();

            if(contentComponentList == null)
            {
                dialogComponentList = new List<DialogGUIBase>(1);
            }
            else
            {
                dialogComponentList = new List<DialogGUIBase>(contentComponentList.Count + 1);
                dialogComponentList.AddRange(contentComponentList);
            }

            //close button and some info
            DialogGUIBase[] footer;
            if (!showVersion && showCloseButton)
            {
                footer = new DialogGUIBase[] 
                    {
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIButton("Close", dismiss),
                    new DialogGUIFlexibleSpace()
                    };
            }
            else if(showVersion && !showCloseButton)
            {
                footer = new DialogGUIBase[]
                    {
                    new DialogGUIFlexibleSpace(),
                    new DialogGUILabel(string.Format("v{0}.{1}", CNCSettings.Instance.MajorVersion, CNCSettings.Instance.MinorVersion), false, false)
                    };
            }
            else
            {
                footer = new DialogGUIBase[]
                    {
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIButton("Close", dismiss),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUILabel(string.Format("v{0}.{1}", CNCSettings.Instance.MajorVersion, CNCSettings.Instance.MinorVersion), false, false)
                    };
            }
            dialogComponentList.Add(new DialogGUIHorizontalLayout(footer));

            //Spawn the dialog
            MultiOptionDialog moDialog = new MultiOptionDialog("",
                                                               dialogTitle,
                                                               HighLogic.UISkin,
                                                               new Rect(normalizedCenterX, normalizedCenterY, windowWidth, windowHeight),
                                                               dialogComponentList.ToArray());

            moDialog.OnUpdate = OnUpdate;
            moDialog.OnResize = OnResize;

            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                                                new Vector2(0.5f, 0.5f),
                                                moDialog,
                                                false,  // persistAcrossScreen
                                                HighLogic.UISkin,
                                                blockBackgroundInputs); // isModal
        }
    }
}
