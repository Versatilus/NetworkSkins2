﻿using System.Collections.Generic;
using System.Reflection;

namespace NetworkSkins.Skins
{
    public class StreetLightModifier : NetworkSkinModifier
    {
        public readonly PropInfo StreetLight;

        public StreetLightModifier(PropInfo streetLight)
        {
            StreetLight = streetLight;
        }

        public override void Apply(NetworkSkin skin)
        {
            if (skin.m_lanes == null) return;

            for (var l = 0; l < skin.m_lanes.Length; l++)
            {
                var laneProps = skin.m_lanes[l]?.m_laneProps?.m_props;
                if (laneProps == null) continue;

                for (var p = 0; p < laneProps.Length; p++)
                {
                    if (IsStreetLightProp(laneProps[p]?.m_finalProp))
                    {
                        skin.UpdateLaneProp(l, p, laneProp =>
                        {
                            laneProp.m_prop = StreetLight;
                            laneProp.m_finalProp = StreetLight;
                        });
                    }
                }
            }
        }

        public static bool HasStreetLights(NetInfo prefab)
        {
            if (prefab.m_lanes == null) return false;

            foreach (var lane in prefab.m_lanes)
            {
                var laneProps = lane?.m_laneProps?.m_props;
                if (laneProps == null) continue;

                foreach (var laneProp in laneProps)
                {
                    if (IsStreetLightProp(laneProp?.m_finalProp))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // nullable
        public static PropInfo GetDefaultStreetLight(NetInfo prefab)
        {
            if (prefab.m_lanes == null) return null;

            foreach (var lane in prefab.m_lanes)
            {
                var laneProps = lane?.m_laneProps?.m_props;
                if (laneProps == null) continue;

                foreach (var laneProp in laneProps)
                {
                    var finalProp = laneProp?.m_finalProp;
                    if (IsStreetLightProp(finalProp))
                    {
                        return finalProp;
                    }
                }
            }

            return null;
        }

        private static bool IsStreetLightProp(PropInfo prefab)
        {
            if (prefab == null) return false;

            if (prefab.m_class.m_service == ItemClass.Service.Road ||
                prefab.m_class.m_subService == ItemClass.SubService.PublicTransportPlane ||
                prefab.name.ToLower().Contains("streetlamp") || prefab.name.ToLower().Contains("streetlight") || prefab.name.ToLower().Contains("lantern"))
            {
                if (prefab.m_effects != null && prefab.m_effects.Length > 0)
                {
                    if (prefab.name.ToLower().Contains("taxiway")) return false;
                    if (prefab.name.ToLower().Contains("runway")) return false;

                    foreach (var effect in prefab.m_effects)
                    {
                        if (effect.m_effect is LightEffect)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
