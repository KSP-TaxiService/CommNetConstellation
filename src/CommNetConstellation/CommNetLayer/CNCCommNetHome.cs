using CommNet;
using CommNetConstellation.UI;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetHome : CommNetHome
    {
        private static readonly Texture2D markTexture = UIUtils.loadImage("groundStationMark");

        public void copyOf(CommNetHome stockHome)
        {
            this.nodeName = stockHome.nodeName;
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            this.comm = stockHome.GetComponentInChildren<CommNode>(); // maybe too early as it is null at beginning
            this.body = stockHome.GetComponentInChildren<CelestialBody>(); // maybe too early as it is null at beginning
        }

        public void OnGUI()
        {
            if (HighLogic.CurrentGame == null)
                return;

            if (!(HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.TRACKSTATION))
                return;

            if (!HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations || !MapView.MapIsEnabled || MapView.MapCamera == null)
                return;

            Vector3d worldPos = ScaledSpace.LocalToScaledSpace(nodeTransform.transform.position);
            if (MapView.MapCamera.transform.InverseTransformPoint(worldPos).z < 0f) return;
            Vector3 pos = PlanetariumCamera.Camera.WorldToScreenPoint(worldPos);
            Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

            if (IsOccluded(nodeTransform.transform.position, this.body))
                return;

            if (!IsOccluded(nodeTransform.transform.position, this.body) && this.IsCamDistanceToWide(nodeTransform.transform.position))
                return;

            Color previousColor = GUI.color;
            GUI.color = Color.red;
            GUI.DrawTexture(screenRect, markTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
        }

        /// <summary>
        /// Checks whether the location is behind the body
        /// Original code by regex from https://github.com/NathanKell/RealSolarSystem/blob/master/Source/KSCSwitcher.cs
        /// </summary>
        private bool IsOccluded(Vector3d loc, CelestialBody body)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
                return false;
            return true;
        }

        /// <summary>
        /// Calculates the distance between the camera position and the ground station, and
        /// returns true if the distance is >= DistanceToHideGroundStations from the settings file.
        /// </summary>
        /// <param name="loc">Position of the ground station</param>
        /// <returns>True if the distance is to wide, otherwise false</returns>
        private bool IsCamDistanceToWide(Vector3d loc)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);
            float distance = Vector3.Distance(camPos, loc);

            if (distance >= CNCSettings.Instance.DistanceToHideGroundStations)
                return true;
            return false;
        }
    }
}
