using CommNet;
using KSP.UI.Screens.Flight;
using KSP.UI.TooltipTypes;
using UnityEngine;
using UnityEngine.UI;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Simple data structure to store the populated data of destroyed stock interface temporarily
    /// </summary>
    public class TelemetryUpdateData
    {
        public CommNetUIModeButton modeButton;
        public Sprite NOSIG;
        public Sprite NOEP;
        public Sprite BLK;
        public Sprite AUP;
        public Sprite ADN;
        public Sprite EP0;
        public Sprite EP1;
        public Sprite EP2;
        public Sprite CK1;
        public Sprite CK2;
        public Sprite CK3;
        public Sprite CP1;
        public Sprite CP2;
        public Sprite CP3;
        public Sprite SS0;
        public Sprite SS1;
        public Sprite SS2;
        public Sprite SS3;
        public Sprite SS4;
        public Image arrow_icon;
        public Image firstHop_icon;
        public Image lastHop_icon;
        public Image control_icon;
        public Image signal_icon;
        public TooltipController_Text firstHop_tooltip;
        public TooltipController_Text arrow_tooltip;
        public TooltipController_Text lastHop_tooltip;
        public TooltipController_Text control_tooltip;
        public TooltipController_SignalStrength signal_tooltip;

        public TelemetryUpdateData(TelemetryUpdate stockTU)
        {
            this.modeButton = stockTU.modeButton;
            this.NOSIG = stockTU.NOSIG;
            this.NOEP = stockTU.NOEP;
            this.BLK = stockTU.BLK;
            this.AUP = stockTU.AUP;
            this.ADN = stockTU.ADN;
            this.EP0 = stockTU.EP0;
            this.EP1 = stockTU.EP1;
            this.EP2 = stockTU.EP2;
            this.CK1 = stockTU.CK1;
            this.CK2 = stockTU.CK2;
            this.CK3 = stockTU.CK3;
            this.CP1 = stockTU.CP1;
            this.CP2 = stockTU.CP2;
            this.CP3 = stockTU.CP3;
            this.SS0 = stockTU.SS0;
            this.SS1 = stockTU.SS1;
            this.SS2 = stockTU.SS2;
            this.SS3 = stockTU.SS3;
            this.SS4 = stockTU.SS4;
            this.arrow_icon = stockTU.arrow_icon;
            this.firstHop_icon = stockTU.firstHop_icon;
            this.lastHop_icon = stockTU.lastHop_icon;
            this.control_icon = stockTU.control_icon;
            this.signal_icon = stockTU.signal_icon;
            this.firstHop_tooltip = stockTU.firstHop_tooltip;
            this.arrow_tooltip = stockTU.arrow_tooltip;
            this.lastHop_tooltip = stockTU.lastHop_tooltip;
            this.control_tooltip = stockTU.control_tooltip;
            this.signal_tooltip = stockTU.signal_tooltip;

            //Doing Reflection to read and save attribute names and values seems too complex,
            //given most of attributes are not primitives
        }
    }

    /// <summary>
    /// As far as I can tell, CommNet telemtry interface is statically set up in Unity Edtior.
    /// It isn't possible to dynamically add new buttons to the interface though it is possible
    /// to "disable" the existing buttons
    /// </summary>
    public class CNCTelemetryUpdate : TelemetryUpdate
    {
        public static new CNCTelemetryUpdate Instance
        {
            get;
            protected set;
        }

        public void copyOf(TelemetryUpdateData stockTUData)
        {
            //replace the mode button
            var customModeButton = stockTUData.modeButton.gameObject.AddComponent<CNCCommNetUIModeButton>();
            customModeButton.copyOf(stockTUData.modeButton);
            UnityEngine.Object.DestroyImmediate(stockTUData.modeButton);
            this.modeButton = customModeButton;

            this.NOSIG = stockTUData.NOSIG;
            this.NOEP = stockTUData.NOEP;
            this.BLK = stockTUData.BLK;
            this.AUP = stockTUData.AUP;
            this.ADN = stockTUData.ADN;
            this.EP0 = stockTUData.EP0;
            this.EP1 = stockTUData.EP1;
            this.EP2 = stockTUData.EP2;
            this.CK1 = stockTUData.CK1;
            this.CK2 = stockTUData.CK2;
            this.CK3 = stockTUData.CK3;
            this.CP1 = stockTUData.CP1;
            this.CP2 = stockTUData.CP2;
            this.CP3 = stockTUData.CP3;
            this.SS0 = stockTUData.SS0;
            this.SS1 = stockTUData.SS1;
            this.SS2 = stockTUData.SS2;
            this.SS3 = stockTUData.SS3;
            this.SS4 = stockTUData.SS4;
            this.arrow_icon = stockTUData.arrow_icon;
            this.firstHop_icon = stockTUData.firstHop_icon;
            this.lastHop_icon = stockTUData.lastHop_icon;
            this.control_icon = stockTUData.control_icon;
            this.signal_icon = stockTUData.signal_icon;
            this.firstHop_tooltip = stockTUData.firstHop_tooltip;
            this.arrow_tooltip = stockTUData.arrow_tooltip;
            this.lastHop_tooltip = stockTUData.lastHop_tooltip;
            this.control_tooltip = stockTUData.control_tooltip;
            this.signal_tooltip = stockTUData.signal_tooltip;
        }

        protected override void Awake()
        {
            //overrode to turn off stock's instance check
            if (TelemetryUpdate.Instance != null && TelemetryUpdate.Instance is TelemetryUpdate)
            {
                UnityEngine.Object.DestroyImmediate(TelemetryUpdate.Instance);
                TelemetryUpdate.Instance = this;
            }
        }
    }
}
