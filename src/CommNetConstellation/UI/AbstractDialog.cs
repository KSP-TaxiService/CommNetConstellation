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
    public abstract class AbstractDialog
    {
        protected bool isDisplayed = false;
        protected string dialogTitle;
        protected int windowWidth;
        protected int windowHeight;
        protected float normalizedCenterX; //0.0f to 1.0f
        protected float normalizedCenterY; //0.0f to 1.0f
        protected bool showCloseButton = false;
        protected bool showVersion = false;

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
            for(int i=0; i<args.Length; i++)
            {
                string arg = args[i];
                if (arg.Equals("showclosebutton"))
                    this.showCloseButton = true;
                else if (arg.Equals("showversion"))
                    this.showVersion = true;
                else
                    CNCLog.Error("AbstractDialog argument '{0}' is unknown",arg);
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

            List<DialogGUIBase> entireComponentList = new List<DialogGUIBase>();

            //content
            List<DialogGUIBase> contentComponentList = drawContentComponents();
            for (int i = 0; i < contentComponentList.Count; i++)
                entireComponentList.Add(contentComponentList.ElementAt(i));

            //close button and some info
            //entireComponentList.Add(new DialogGUISpace(4));
            if (showVersion || showCloseButton)
            {
                entireComponentList.Add(new DialogGUIHorizontalLayout(
                                            (showCloseButton) ?
                                            new DialogGUIBase[]
                                            {
                                            new DialogGUIFlexibleSpace(),
                                            new DialogGUIButton("Close", dismiss),
                                            new DialogGUIFlexibleSpace(),
                                            new DialogGUILabel((!showVersion)?"":string.Format("v{0}.{1}", CNCSettings.Instance.MajorVersion, CNCSettings.Instance.MinorVersion), false, false)
                                            }
                                            :
                                            new DialogGUIBase[]
                                            {
                                            new DialogGUIFlexibleSpace(),
                                            new DialogGUILabel((!showVersion)?"":string.Format("v{0}.{1}", CNCSettings.Instance.MajorVersion, CNCSettings.Instance.MinorVersion), false, false)
                                            }
                                        ));
            }

            //Spawn the dialog
            MultiOptionDialog moDialog = new MultiOptionDialog("",
                                                               dialogTitle,
                                                               HighLogic.UISkin,
                                                               new Rect(normalizedCenterX, normalizedCenterY, windowWidth, windowHeight),
                                                               entireComponentList.ToArray());

            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                                                new Vector2(0.5f, 0.5f),
                                                moDialog,
                                                false, // true = ?
                                                HighLogic.UISkin);
        }
    }
}
