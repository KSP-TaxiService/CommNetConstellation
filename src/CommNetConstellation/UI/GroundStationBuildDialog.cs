using System;
using System.Collections.Generic;
using CommNetConstellation.CommNetLayer;
using KSP.Localization;
using UnityEngine;
using UnityEngine.UI;
using CommNetConstellation.UI.DialogGUI;

namespace CommNetConstellation.UI
{
    public class GroundStationBuildDialog : AbstractDialog
    {
        private static readonly Texture2D L0PicTexture = UIUtils.loadImage("GroundStationL0Pic");
        private static readonly Texture2D L1PicTexture = UIUtils.loadImage("GroundStationL1Pic");
        private static readonly Texture2D L2PicTexture = UIUtils.loadImage("GroundStationL2Pic");
        private static readonly Texture2D L3PicTexture = UIUtils.loadImage("GroundStationL3Pic");
        private static readonly Texture2D upgradeArrowTexture = UIUtils.loadImage("upgradeArrow");

        private CNCCommNetHome hostStation;
        private DialogGUIImage currentTexture, nextTexture;
        private string description = Localizer.Format("#CNC_AntennaSetup_DescText1");//"Something"
        private Callback<string> updateCallback;

        public GroundStationBuildDialog (string title, CNCCommNetHome thisStation, Callback<string> updateCallback) : base("gsBuild",
                                                                                                                title,
                                                                                                                0.5f, //x
                                                                                                                0.5f, //y
                                                                                                                300, //width
                                                                                                                330, //height
                                                                                                                new DialogOptions[] { DialogOptions.HideCloseButton })
        {
            this.hostStation = thisStation;
            this.updateCallback = updateCallback;
            this.description = Localizer.Format("#CNC_GroundStationBuild_desc", thisStation.stationName);//"You are upgrading the ground station '{0}'."

            this.GetInputLocks();
        }

        protected override void OnPreDismiss()
        {
            updateCallback(this.hostStation.ID);
            this.ReleaseInputLocks();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { new DialogGUILabel(this.description + "\n\n", false, false) }));

            //Current group
            DialogGUILabel currentLevelLabel = new DialogGUILabel("<b>" + Localizer.Format("#CNC_GroundStationBuild_currentLevelLabel") + "</b>");//Current tech level
            currentTexture = new DialogGUIImage(new Vector2(100, 100), Vector2.zero, Color.white, getLevelTexture(this.hostStation.TechLevel));
            DialogGUILabel currentPower = new DialogGUILabel(currentPowerFunc);
            DialogGUIVerticalLayout currentLevelGroup = new DialogGUIVerticalLayout(100, 100, 4, new RectOffset(), TextAnchor.MiddleCenter, 
                new DialogGUIBase[] { currentLevelLabel, currentTexture, currentPower });

            //Upgrade arrow
            DialogGUIImage arrowTexture = new DialogGUIImage(new Vector2(40, 40), Vector2.zero, Color.white, upgradeArrowTexture);

            //Next group
            DialogGUILabel nextLevelLabel = new DialogGUILabel("<b>" + Localizer.Format("#CNC_GroundStationBuild_nextLevelLabel") + "</b>");//Next tech level
            nextTexture = new DialogGUIImage(new Vector2(100, 100), Vector2.zero, Color.white, getLevelTexture((short)(this.hostStation.TechLevel + 1)));
            DialogGUILabel nextPower = new DialogGUILabel(nextPowerFunc);
            DialogGUIVerticalLayout nextLevelGroup = new DialogGUIVerticalLayout(100, 100, 4, new RectOffset(), TextAnchor.MiddleCenter, 
                new DialogGUIBase[] { nextLevelLabel, nextTexture, nextPower });

            listComponments.Add(new CustomDialogGUIScrollList(new Vector2(300-10, 150), false, false, 
                                    new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { currentLevelGroup, arrowTexture, nextLevelGroup })));

            //Requirements
            if (Funding.Instance != null) //only available in Career mode
            {
                DialogGUILabel costLabel = new DialogGUILabel(costFunc);
                DialogGUILabel availableLabel = new DialogGUILabel(availableFunc);
                listComponments.Add(new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { costLabel }));
                listComponments.Add(new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { availableLabel }));
            }
            listComponments.Add(new DialogGUISpace(10));

            DialogGUIButton upgradeButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_Upgradebutton"), onClickUpgrade, false);//Upgrade
            upgradeButton.OptionInteractableCondition = () => this.hostStation.TechLevel < CNCSettings.Instance.GroundStationUpgradesCount ? true : false;
            DialogGUIButton closeButton = new DialogGUIButton(Localizer.Format("#CNC_Generic_Close"), delegate { this.dismiss(); }, false);//Close
            DialogGUIHorizontalLayout actionGroup = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), upgradeButton, closeButton, new DialogGUIFlexibleSpace() });
            listComponments.Add(actionGroup);

            return listComponments;
        }

        private string currentPowerFunc()
        {
            double power = this.hostStation.TechLevel <= 0 ? 0.0 : CNCSettings.Instance.GroundStationUpgradeablePowers[this.hostStation.TechLevel - 1];
            return Localizer.Format("#CNC_GroundStationBuild_DSNpower") + string.Format(" {0:0.00}", UIUtils.RoundToNearestMetricFactor(power, 2));//DSN Power: {0:0.00}
        }

        private string nextPowerFunc()
        {
            if(this.hostStation.TechLevel >= CNCSettings.Instance.GroundStationUpgradesCount)
            {
                return Localizer.Format("#CNC_GroundStationBuild_DSNpowerNil");//DSN Power: Nil
            }
            else
            {
                double power = CNCSettings.Instance.GroundStationUpgradeablePowers[this.hostStation.TechLevel];
                return Localizer.Format("#CNC_GroundStationBuild_DSNpower") + string.Format(" {0:0.00}", UIUtils.RoundToNearestMetricFactor(power, 2));//DSN Power: {0:0.00}
            }
            
        }

        private string costFunc()
        {
            if(this.hostStation.TechLevel >= CNCSettings.Instance.GroundStationUpgradesCount)
            {
                return Localizer.Format("#CNC_GroundStationBuild_InvoiceNil");//Invoice: Nil
            }
            else
            {
                return Localizer.Format("#CNC_GroundStationBuild_Invoice") + string.Format(" {0:n0}", CNCSettings.Instance.GroundStationUpgradeableCosts[this.hostStation.TechLevel]);//Invoice: {0:n0}
            }
        }

        private string availableFunc()
        {
            if(Funding.Instance == null)
            {
                return ""; //failsafe
            }

            string color = "green";
            if (this.hostStation.TechLevel < CNCSettings.Instance.GroundStationUpgradesCount)
            {
                int cost = CNCSettings.Instance.GroundStationUpgradeableCosts[this.hostStation.TechLevel];
                if (Funding.Instance.Funds < cost)
                {
                    color = "red";
                }
            }

            return Localizer.Format("#CNC_GroundStationBuild_FundsAvailable") + string.Format(" <color={1}>{0:n0}</color>", Funding.Instance.Funds, color);//Available Funds: {0:n0}
        }

        private void onClickUpgrade()
        {
            int cost = CNCSettings.Instance.GroundStationUpgradeableCosts[this.hostStation.TechLevel];
            if (Funding.Instance != null && Funding.Instance.Funds < cost)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>" + Localizer.Format("#CNC_GroundStationBuild_costError") + "</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER);//Insufficient Funds
                ScreenMessages.PostScreenMessage(msg);
            }
            else
            {
                this.hostStation.incrementTechLevel();
                updateCallback.Invoke(this.hostStation.ID);

                if (Funding.Instance != null)
                {
                    Funding.Instance.AddFunds(-1.0 * cost, TransactionReasons.StructureConstruction);
                }

                currentTexture.uiItem.GetComponent<RawImage>().texture = getLevelTexture(this.hostStation.TechLevel);
                nextTexture.uiItem.GetComponent<RawImage>().texture = getLevelTexture((short)(this.hostStation.TechLevel < CNCSettings.Instance.GroundStationUpgradesCount ? (this.hostStation.TechLevel + 1) : this.hostStation.TechLevel));
            }
        }

        private Texture2D getLevelTexture(short  level)
        {
            switch (level)
            {
                case 0:
                    return L0PicTexture;
                case 1:
                    return L1PicTexture;
                case 2:
                    return L2PicTexture;
                case 3:
                    return L3PicTexture;
                default:
                    CNCLog.Verbose("No texture found for Tech Level {0}!", level);
                    return L3PicTexture;
            }
        }
    }
}
