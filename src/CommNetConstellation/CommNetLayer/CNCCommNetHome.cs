using CommNet;
using CommNetConstellation.UI;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Customise the home nodes
    /// </summary>
    public class CNCCommNetHome : CommNetHome
    {
        private static readonly Texture2D markTexture = UIUtils.loadImage("groundStationMark");
        private static GUIStyle groundStationHeadline;
        private bool loadCompleted = false;

        public void copyOf(CommNetHome stockHome)
        {
            CNCLog.Debug("CommNet Home '{0}' added", stockHome.nodeName);

            this.nodeName = stockHome.nodeName;
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            this.body = stockHome.GetComponentInParent<CelestialBody>();

            //comm, lat, alt, lon are initialised by CreateNode() later

            groundStationHeadline = new GUIStyle(HighLogic.Skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.yellow }
            };

            loadCompleted = true;
        }

        /// <summary>
        /// Draw graphic components on screen like RemoteTech's ground-station marks
        /// </summary>
        public void OnGUI()
        {
            if (HighLogic.CurrentGame == null || !loadCompleted)
                return;

            if (!(HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.TRACKSTATION))
                return;

            if ((!HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations && !this.isKSC) || !MapView.MapIsEnabled || MapView.MapCamera == null)
                return;

            Vector3d worldPos = ScaledSpace.LocalToScaledSpace(nodeTransform.transform.position);

            if (MapView.MapCamera.transform.InverseTransformPoint(worldPos).z < 0f)
                return;

            Vector3 position = PlanetariumCamera.Camera.WorldToScreenPoint(worldPos);
            Rect groundStationRect = new Rect((position.x - 8), (Screen.height - position.y) - 8, 16, 16);

            if (isOccluded(nodeTransform.transform.position, this.body))
                return;

            if (!isOccluded(nodeTransform.transform.position, this.body) && MapView.MapCamera.Distance > MapView.fetch.max3DlineDrawDist)
                return;

            //draw the dot
            Color previousColor = GUI.color;
            GUI.color = Color.red;
            GUI.DrawTexture(groundStationRect, markTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;

            //draw the headline below the dot
            if (UIUtils.ContainsMouse(groundStationRect))
            {
                Rect headlineRect = groundStationRect;
                Vector2 nameDim = CNCCommNetHome.groundStationHeadline.CalcSize(new GUIContent(this.nodeName));
                headlineRect.x -= nameDim.x/2;
                headlineRect.y -= nameDim.y + 5;
                headlineRect.width = nameDim.x;
                headlineRect.height = nameDim.y;
                GUI.Label(headlineRect, this.nodeName, CNCCommNetHome.groundStationHeadline);
            }
        }

        /// <summary>
        /// Check whether this vector3 location is behind the body
        /// Original code by regex from https://github.com/NathanKell/RealSolarSystem/blob/master/Source/KSCSwitcher.cs
        /// </summary>
        private bool isOccluded(Vector3d position, CelestialBody body)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - position, body.position - position) > 90)
                return false;
            return true;
        }
    }
}
