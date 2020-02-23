using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

/* References
 * http://forum.kerbalspaceprogram.com/index.php?/topic/149324-popupdialog-and-the-dialoggui-classes/
 * http://forum.kerbalspaceprogram.com/index.php?/topic/151354-unity-ui-creation-tutorial/
 * http://forum.kerbalspaceprogram.com/index.php?/topic/151270-gui-scrolling-problem/
 */

namespace CommNetConstellation.UI
{
    public enum DialogOptions
    {
        HideCloseButton,
        ShowVersion,
        AllowBgInputs,
        NonDraggable
    };

    /// <summary>
    /// Easy-to-use popup dialog with some customisations
    /// </summary>
    public abstract class AbstractDialog
    {
        protected string dialogHandler;
        protected string dialogTitle;
        protected int windowWidth;
        protected int windowHeight;
        protected float normalizedCenterX; //0.0f to 1.0f
        protected float normalizedCenterY; //0.0f to 1.0f

        protected string dismissButtonText = Localizer.Format("#CNC_Generic_Close");//"Close"
        protected bool showCloseButton = true;
        protected bool showVersion = false;
        protected bool blockBackgroundInputs = true;
        protected bool draggable = true;

        protected PopupDialog popupDialog = null;

        public AbstractDialog(string dialogUniqueHandler, string dialogTitle, float normalizedCenterX, float normalizedCenterY, int windowWidth, int windowHeight, DialogOptions[] args)
        {
            this.dialogHandler = dialogUniqueHandler;
            this.dialogTitle = dialogTitle;
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
            this.normalizedCenterX = normalizedCenterX;
            this.normalizedCenterY = normalizedCenterY;

            processArguments(args);
        }

        protected abstract List<DialogGUIBase> drawContentComponents();
        protected virtual void OnAwake(System.Object[] args) { }
        protected virtual void OnPreDismiss() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnResize() { } // no idea how resizing works

        /// <summary>
        /// Create and draw the components of a given layout
        /// </summary>
        public static void registerLayoutComponents(DialogGUILayoutBase layout)
        {
            if (layout.children.Count < 1)
                return;

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(layout.uiItem.gameObject.transform);
            for (int i = 0; i < layout.children.Count; i++)
            {
                if (!(layout.children[i] is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                    layout.children[i].Create(ref stack, HighLogic.UISkin); // recursively create child's children
            }
        }

        /// <summary>
        /// Delete the components of a given layout
        /// </summary>
        public static void deregisterLayoutComponents(DialogGUILayoutBase layout)
        {
            recursiveLayoutDeletion(layout); // need to delete layout's children since no recursive deletion found
        }

        /// <summary>
        /// Recusively delete every layout's children
        /// </summary>
        private static void recursiveLayoutDeletion(DialogGUILayoutBase layout)
        {
            if (layout.children.Count < 1)
                return;

            int size = layout.children.Count;
            for (int i = size - 1; i >= 0; i--)
            {
                DialogGUIBase thisChild = layout.children[i];
                if (thisChild is DialogGUILayoutBase)
                {
                    recursiveLayoutDeletion(thisChild as DialogGUILayoutBase);
                }

                if (!(thisChild is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                {
                    layout.children.RemoveAt(i);
                    thisChild.uiItem.gameObject.DestroyGameObjectImmediate();
                }
            }
        }

        /// <summary>
        /// Spawn the dialog without any argument
        /// </summary>
        public void launch()
        {
            launch(new System.Object[] { });
        }

        /// <summary>
        /// Spawn the dialog with arguments passed
        /// </summary>
        public void launch(System.Object[] args)
        {
            if (popupDialog != null)
                return;

            popupDialog = spawnDialog();
            popupDialog.OnDismiss = new Callback(dismiss);
            OnAwake(args);
        }

        /// <summary>
        /// Close and deallocate the dialog
        /// </summary>
        public void dismiss()
        {
            if (popupDialog != null)
            {
                OnPreDismiss();
                popupDialog.Dismiss();
                popupDialog = null;
            }
        }

        /// <summary>
        /// Read the constructor arguments
        /// </summary>
        private void processArguments(DialogOptions[] args)
        {
            if (args == null)
                return;

            for(int i=0; i<args.Length; i++)
            {
                switch (args[i])
                {
                    case DialogOptions.HideCloseButton:
                        this.showCloseButton = false;
                        break;
                    case DialogOptions.ShowVersion:
                        this.showVersion = true;
                        break;
                    case DialogOptions.AllowBgInputs:
                        this.blockBackgroundInputs = false;
                        break;
                    case DialogOptions.NonDraggable:
                        this.draggable = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Build and return the not-spawned dialog
        /// </summary>
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
                    new DialogGUIButton(dismissButtonText, dismiss),
                    new DialogGUIFlexibleSpace()
                    };
                dialogComponentList.Add(new DialogGUIHorizontalLayout(footer));
            }
            else if(showVersion && !showCloseButton)
            {
                footer = new DialogGUIBase[]
                    {
                    new DialogGUIFlexibleSpace(),
                    new DialogGUILabel(GameUtils.Version, false, false)
                    };
                dialogComponentList.Add(new DialogGUIHorizontalLayout(footer));
            }
            else if(showVersion && showCloseButton)
            {
                footer = new DialogGUIBase[]
                    {
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIButton(dismissButtonText, dismiss),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUILabel(GameUtils.Version, false, false)
                    };
                dialogComponentList.Add(new DialogGUIHorizontalLayout(footer));
            }

            //Spawn the dialog
            MultiOptionDialog moDialog = new MultiOptionDialog(this.dialogHandler, // unique name for every dialog
                                                               "",
                                                               dialogTitle,
                                                               HighLogic.UISkin,
                                                               new Rect(normalizedCenterX, normalizedCenterY, windowWidth, windowHeight),
                                                               dialogComponentList.ToArray());

            moDialog.OnUpdate = OnUpdate;
            moDialog.OnResize = OnResize;

            PopupDialog newDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                                                                    new Vector2(0.5f, 0.5f),
                                                                    moDialog,
                                                                    false,  // persistAcrossScreen
                                                                    HighLogic.UISkin,
                                                                    blockBackgroundInputs); // isModal
            newDialog.SetDraggable(draggable);
            return newDialog;
        }

        /// <summary>
        /// Release UI locks (enable usage of UI buttons).
        /// </summary>
        /// Ported from RemoteTech codebase
        protected void ReleaseInputLocks()
        {
            InputLockManager.RemoveControlLock("CNCLockStaging");
            //InputLockManager.RemoveControlLock("CNCLockSAS");
            //InputLockManager.RemoveControlLock("CNCLockRCS");
            InputLockManager.RemoveControlLock("CNCLockActions");
            //InputLockManager.RemoveControlLock("CNCLockMap");
            InputLockManager.RemoveControlLock("CNCLockKeyboard");
        }

        /// <summary>
        /// Acquire UI locks (disable usage of UI buttons).
        /// </summary>
        /// Ported from RemoteTech codebase
        protected void GetInputLocks()
        {
            InputLockManager.SetControlLock(ControlTypes.STAGING, "CNCLockStaging");
            //InputLockManager.SetControlLock(ControlTypes.SAS, "CNCLockSAS");
            //InputLockManager.SetControlLock(ControlTypes.RCS, "CNCLockRCS");
            InputLockManager.SetControlLock(ControlTypes.GROUPS_ALL, "CNCLockActions");
            //InputLockManager.SetControlLock(ControlTypes.MAP, "CNCLockMap");
            InputLockManager.SetControlLock(ControlTypes.KEYBOARDINPUT, "CNCLockKeyboard");
        }
    }
}
