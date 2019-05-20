using UnityEngine;

namespace CommNetConstellation.UI.DialogGUI
{
    /// <summary>
    /// Subclass of DialogGUIScrollList to customise inner functions/objects
    /// </summary>
    public class CustomDialogGUIScrollList : DialogGUIScrollList
    {
        protected bool defaultTop = false;
        protected bool defaultBottom = false;

        public CustomDialogGUIScrollList(Vector2 size, bool hScroll, bool vScroll, DialogGUILayoutBase layout) : 
            base(size, hScroll, vScroll, layout)
        {
            SetDefaultScrollToTop();
        }

        public CustomDialogGUIScrollList(Vector2 size, Vector2 contentSize, bool hScroll, bool vScroll, DialogGUILayoutBase layout) : 
            base(size, contentSize, hScroll, vScroll, layout)
        {
            SetDefaultScrollToTop();
        }

        public override void Update()
        {
            base.Update();

            if (defaultTop)
            {
                ScrollToTop();
                defaultTop = false;
            }
            if (defaultBottom)
            {
                ScrollToBottom();
                defaultBottom = false;
            }
        }

        public void ScrollToTop()
        {
            this.scrollRect.content.pivot = new Vector2(0, 1);
        }

        public void ScrollToBottom()
        {
            this.scrollRect.content.pivot = new Vector2(0, 0);
        }

        public void SetDefaultScrollToTop()
        {
            defaultTop = true;
        }

        public void SetDefaultScrollToBottom()
        {
            defaultBottom = true;
        }
    }
}
