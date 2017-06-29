using System.Collections.Generic;
using CommNet;
using UnityEngine;
using static CommNetConstellation.CommNetLayer.CNCCommNetVessel;

namespace CommNetConstellation.UI.VesselMgtTools
{
    public class UpdateListTool : AbstractMgtTool
    {
        private UIStyle style;

        public UpdateListTool(CommNetVessel thisVessel) : base(thisVessel, "updatelist", "Update List")
        {
            this.style = new UIStyle();
            this.style.alignment = TextAnchor.MiddleLeft;
            this.style.fontStyle = FontStyle.Normal;
            this.style.normal = new UIStyleState();
            this.style.normal.textColor = Color.white;
        }

        public override List<DialogGUIBase> getContentComponents()
        {
            List<DialogGUIBase> layout = new List<DialogGUIBase>();

            DialogGUILabel msgLbl = new DialogGUILabel("Decide how the vessel's frequency list is updated whenever one antenna is changed (eg deployed/retracted or frequency change)\n");
            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { msgLbl }));

            DialogGUIToggleGroup toggleGrp = new DialogGUIToggleGroup();
            DialogGUIVerticalLayout nameColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout descriptionColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            DialogGUIToggle toggleBtn1 = new DialogGUIToggle((cncVessel.FreqListOperation == FrequencyListOperation.AutoBuild) ? true : false, "", delegate (bool b) { ListOperationSelected(b, FrequencyListOperation.AutoBuild); }, 20, 32);
            DialogGUILabel nameLabel1 = new DialogGUILabel("Auto Build", style); nameLabel1.size = new Vector2(80, 32);
            DialogGUILabel descriptionLabel1 = new DialogGUILabel("Rebuild the list from all antennas automatically", style); descriptionLabel1.size = new Vector2(350, 32);
            toggleGrp.AddChild(toggleBtn1);
            nameColumn.AddChild(nameLabel1);
            descriptionColumn.AddChild(descriptionLabel1);

            DialogGUIToggle toggleBtn2 = new DialogGUIToggle((cncVessel.FreqListOperation == FrequencyListOperation.LockList) ? true : false, "", delegate (bool b) { ListOperationSelected(b, FrequencyListOperation.LockList); }, 20, 32);
            DialogGUILabel nameLabel2 = new DialogGUILabel("Lock List", style); nameLabel2.size = new Vector2(80, 32);
            DialogGUILabel descriptionLabel2 = new DialogGUILabel("Disallow any change in the current list", style); descriptionLabel2.size = new Vector2(350, 32);
            toggleGrp.AddChild(toggleBtn2);
            nameColumn.AddChild(nameLabel2);
            descriptionColumn.AddChild(descriptionLabel2);

            DialogGUIToggle toggleBtn3 = new DialogGUIToggle((cncVessel.FreqListOperation == FrequencyListOperation.UpdateOnly) ? true : false, "", delegate (bool b) { ListOperationSelected(b, FrequencyListOperation.UpdateOnly); }, 20, 32);
            DialogGUILabel nameLabel3 = new DialogGUILabel("Update Only", style); nameLabel3.size = new Vector2(80, 32);
            DialogGUILabel descriptionLabel3 = new DialogGUILabel("Update the affected frequency in the list only (not yet)", style); descriptionLabel3.size = new Vector2(350, 32);
            toggleGrp.AddChild(toggleBtn3);
            nameColumn.AddChild(nameLabel3);
            descriptionColumn.AddChild(descriptionLabel3);

            layout.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft, toggleGrp), nameColumn, descriptionColumn }));

            return layout;
        }

        private void ListOperationSelected(bool b, FrequencyListOperation operation)
        {
            if (b)
            {
                cncVessel.FreqListOperation = operation;
            }
        }
    }
}
