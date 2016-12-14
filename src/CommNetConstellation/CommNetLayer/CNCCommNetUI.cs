using CommNet;
using System.Collections.Generic;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetUI : CommNetUI
    {
        GameObject m_SphereObj;
        MapObject m_MapObject;
        MapObject m_currentMapObject;
        Renderer m_renderer;

        public CNCCommNetUI()
        {
            base.colorHigh = new Color(0f, 0f, 1f, 1f); // blue
        }

        protected override void Start()
        {
            base.Start();

            // Create a sphere object
            m_SphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_SphereObj.layer = 10;

            // disable collider for the sphere
            var collider = m_SphereObj.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);

            // get object renderer and apply attributes
            m_renderer = m_SphereObj.GetComponent<Renderer>();
            var shader = Shader.Find("KSP/Diffuse");

            m_renderer.material = new Material(shader);
            m_renderer.material.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
            m_renderer.receiveShadows = false;
            m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_renderer.enabled = false;

            GameEvents.onPlanetariumTargetChanged.Add(OnPlanetariumTargetChanged);
        }

        protected override void UpdateDisplay()
        {
            base.UpdateDisplay();

            //now time for craft sphere-markers...
            if (m_MapObject != null)
            {
                // TEST ; scale down a bit if chosen target is a vessel
                float scale_multiplier = 0f;
                Color object_color = Color.white;
                switch (m_MapObject.type)
                {
                    case MapObject.ObjectType.Vessel:
                        scale_multiplier = 20f;
                        object_color = new Color(0f, 0f, 1f, 0.4f);
                        break;

                    default:
                        return;
                }

                m_SphereObj.transform.position = ScaledSpace.LocalToScaledSpace(m_MapObject.transform.position);
                m_SphereObj.transform.parent = m_MapObject.transform;
                m_SphereObj.transform.localScale = Vector3.one * scale_multiplier;
                m_SphereObj.transform.localPosition = Vector3.zero;
                m_SphereObj.transform.localRotation = Quaternion.identity;
                var renderer = m_SphereObj.GetComponent<Renderer>();
                renderer.material.color = object_color;
                renderer.enabled = true;

                if (m_MapObject != m_currentMapObject)
                {
                    m_currentMapObject = m_MapObject;
                    CNCLog.Debug("m_MapObject: " + m_MapObject.name);
                    CNCLog.Debug("position: " + m_MapObject.transform.position.ToString());
                    CNCLog.Debug("localPosition: " + m_MapObject.transform.localPosition.ToString());
                    CNCLog.Debug("localScale: " + m_MapObject.transform.localScale.ToString());

                    CNCLog.Debug("m_SphereObj: " + m_SphereObj.name);
                    CNCLog.Debug("position: " + m_SphereObj.transform.position.ToString());
                    CNCLog.Debug("localPosition: " + m_SphereObj.transform.localPosition.ToString());
                    CNCLog.Debug("localScale: " + m_SphereObj.transform.localScale.ToString());
                }
            }
            else
            {
                var renderer = m_SphereObj.GetComponent<Renderer>();
                renderer.enabled = false;
            }

        }

        private void OnPlanetariumTargetChanged(MapObject mapObject)
        {
            if (mapObject == null)
            {
                return;
            }

            switch (mapObject.type)
            {
                case MapObject.ObjectType.Vessel:
                    CNCLog.Debug("OnPlanetariumTargetChanged: Vessel: " + mapObject.name);
                    m_MapObject = mapObject;
                    break;
            }
        }
    }
}
