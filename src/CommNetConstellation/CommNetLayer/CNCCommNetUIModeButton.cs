using CommNet;
using KSP.Localization;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetUIModeButton : CommNetUIModeButton
    {
        private bool initialised = false;

        public void copyOf(CommNetUIModeButton stockTUButton)
        {
            this.button = stockTUButton.button;
            this.stateImage = stockTUButton.stateImage;
            this.tooltip = stockTUButton.tooltip;
            this.initialised = true;
        }

        protected override void Awake()
        {
            if (CommNet.CommNetScenario.CommNetEnabled)
            {
                base.gameObject.SetActive(true);
                //GameEvents.CommNet.OnNetworkInitialized.Add(new EventVoid.OnEvent(this.OnNetworkInitialized));
                //Issue: For unknown reason, OnNetworkInitialized() is never called in tracking station or flight
            }
        }

        protected override void OnDestroy()
        {
            //GameEvents.CommNet.OnNetworkInitialized.Remove(new EventVoid.OnEvent(this.OnNetworkInitialized)); //see Awake()
        }

        public override void UpdateUI()
        {
            if (this.initialised)
            {
                base.UpdateUI();
            }
        }
    }
}
