using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetHome : CommNetHome
    {
        private Texture2D markTexture;

        public void copyOf(CommNetHome stockHome)
        {
            this.nodeName = stockHome.nodeName;
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            this.comm = stockHome.GetComponentInChildren<CommNode>(); // maybe too early as it is null at beginning
            this.body = stockHome.GetComponentInChildren<CelestialBody>(); // maybe too early as it is null at beginning
        }

        protected override void Start()
        {
            base.Start();
            markTexture = CNCUtils.loadImage("mark");
        }

        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            if (this.comm != null)
            {
                CNCCommNetNetwork.Add(this.comm);
            }
        }

        public void OnGUI()
        {
            if (HighLogic.CurrentGame != null && (HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations || this.isKSC))
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    return;

                var worldPos = ScaledSpace.LocalToScaledSpace(nodeTransform.transform.position);
                if (MapView.MapCamera.transform.InverseTransformPoint(worldPos).z < 0f) return;
                Vector3 pos = PlanetariumCamera.Camera.WorldToScreenPoint(worldPos);
                var screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

                // Hide the current ISatellite if it is behind its body
                if (IsOccluded(nodeTransform.transform.position, this.body))
                    return;

                if (!IsOccluded(nodeTransform.transform.position, this.body) && this.IsCamDistanceToWide(nodeTransform.transform.position))
                    return;

                Color pushColor = GUI.color;
                // tint the white mark.png into the defined color
                GUI.color = Color.red;
                // draw the mark.png
                GUI.DrawTexture(screenRect, markTexture, ScaleMode.ScaleToFit, true);
                GUI.color = pushColor;
            }
        }

        /// <summary>
        /// Checks whether the location is behind the body
        /// Original code by regex from https://github.com/NathanKell/RealSolarSystem/blob/master/Source/KSCSwitcher.cs
        /// </summary>
        private bool IsOccluded(Vector3d loc, CelestialBody body)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - loc, body.position - loc) > 90) { return false; }
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

            // distance to wide?
            if (distance >= CNCSettings.Instance.DistanceToHideGroundStations)
                return true;

            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.comm != null)
            {
                CNCCommNetNetwork.Remove(this.comm);
            }
        }

        protected override void CreateNode()
        {
            base.CreateNode();
            if (HighLogic.CurrentGame != null && !HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations && !this.isKSC)
            {
                if (this.comm != null)
                {
                    CNCCommNetNetwork.Remove(this.comm);
                    this.comm = null;
                }
                return;
            }
        }
    }
}
