using CommNet;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetNetwork : CommNetNetwork
    {
        /*TODO Replace CommNetwork with CNCCommNetwork
        public override CommNetwork CommNet
        {
            get
            {
                return base.CommNet;
            }

            set
            {
                base.CommNet = value;
            }
        }
        */

        protected override void Update()
        {
            //CNCLog.Debug("CNCCommNetNetwork.Update()");
            base.Update();
        }

    }
}
