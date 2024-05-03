using HarmonyLib;
using UnityEngine;

namespace Collective.Patches
{
    [HarmonyPatch(typeof(DayCycleManager))]
    [HarmonyPatch("UpdateLighting")]
    public class UpdatedLightingPatch
    {
        private static Material daySkyboxMaterial; // Store the original skybox material
        private static Material nightSkyboxMaterial; // Store a material for night skybox

        private static float sunriseStart = 6f; // 6:00 AM
        private static float middayStart = 11f; // 12:00 PM
        private static float sunsetStart = 18f; // 6:00 PM

        [HarmonyPrefix]
        public static bool UpdateLighting(DayCycleManager __instance)
        {
            // Load skybox materials if not already loaded
            if (daySkyboxMaterial == null || nightSkyboxMaterial == null)
                LoadSkyboxMaterials(__instance);
             
            var currentNormalTime = Collective.GetNormalizedTime();
            var currentTime = currentNormalTime.Hour;
            
            // Determine the current lighting phase
            if (currentTime >= sunsetStart || currentTime < sunriseStart) // Nightsta
            {
                SetNightLighting(__instance, currentTime);
            }
            else if (currentTime >= sunriseStart && currentTime < middayStart) // Sunrise
            {
                SetSunriseLighting(__instance, currentTime);
            }
            else if (currentTime >= middayStart && currentTime < sunsetStart) // Midday to Sunset
            {
                SetMiddayToSunsetLighting(__instance, currentTime);
            }

            return false;
        }

        private static void SetNightLighting(DayCycleManager instance, float currentTime)
        {
            // Calculate the transition percentage 't' based on the current time
            float t = Mathf.Clamp01((currentTime - sunsetStart) / (24f - sunsetStart));

            // Smooth color interpolation using a power function
            float smoothT = Mathf.Pow(t, 1.5f); // Adjust the power for different smoothness

            // Adjust lighting properties for night
            instance.m_DirectionalLight.intensity = 0.05f; // Set a low intensity to simulate darkness

            // Use a lower power value to ensure a smoother transition with less color change
            Color offBlack = new Color(0.05f, 0.05f, 0.05f);
            RenderSettings.ambientSkyColor = Color.Lerp(offBlack, Color.black, smoothT);
            RenderSettings.ambientEquatorColor = Color.Lerp(offBlack, Color.black, smoothT);
            RenderSettings.ambientGroundColor = Color.Lerp(offBlack, Color.black, smoothT);
 
            RenderSettings.skybox = nightSkyboxMaterial;
        }

        private static void SetSunriseLighting(DayCycleManager instance, float currentTime)
        {
            // Calculate the transition percentage 't' based on the current time
            float t = Mathf.Clamp01((currentTime - sunriseStart) / (middayStart - sunriseStart));

            // Smooth color interpolation using a power function
            float smoothT = Mathf.Pow(t, 1.5f); // Adjust the power for different smoothness

            // Use a lower power value to ensure a smoother transition with less color change
            Color offBlack = new Color(0.05f, 0.05f, 0.05f);
            instance.m_DirectionalLight.intensity = Mathf.Lerp(0.05f, 1f, t); // Smooth intensity increase
            RenderSettings.ambientSkyColor = Color.Lerp(offBlack, Color.white, smoothT);
            RenderSettings.ambientEquatorColor = Color.Lerp(offBlack, Color.white, smoothT);
            RenderSettings.ambientGroundColor = Color.Lerp(offBlack, Color.white, smoothT);

            if (currentTime > 7) RenderSettings.skybox = daySkyboxMaterial;
        }

        private static void SetMiddayToSunsetLighting(DayCycleManager instance, float currentTime)
        {
            // Calculate the transition percentage 't' based on the current time
            float t = Mathf.Clamp01((currentTime - middayStart) / (sunsetStart - middayStart));

            // Smooth color interpolation using a power function
            float smoothT = Mathf.Pow(t, 1.5f); // Adjust the power for different smoothness

            // Use a lower power value to ensure a smoother transition with less color change
            Color offBlack = new Color(0.05f, 0.05f, 0.05f);
            instance.m_DirectionalLight.intensity = Mathf.Lerp(0.05f, 1f, t); // Intensity increases gradually

            // Only start transitioning to night after 6 PM
            if (currentTime >= 16)
            {
                RenderSettings.ambientSkyColor = Color.Lerp(Color.white, offBlack, smoothT); // Sky color changes
                RenderSettings.ambientEquatorColor = Color.Lerp(Color.white, offBlack, smoothT); // Equator color changes
                RenderSettings.ambientGroundColor = Color.Lerp(Color.white, offBlack, smoothT); // Ground color changes

                if (currentTime < 17) 
                    RenderSettings.skybox = daySkyboxMaterial;
                else
                    RenderSettings.skybox = nightSkyboxMaterial;
            }
        }
        private static void LoadSkyboxMaterials(DayCycleManager instance)
        {
            // Load the original skybox material
            daySkyboxMaterial = instance.m_SkyboxMaterial;
            nightSkyboxMaterial = new Material(RenderSettings.skybox.shader);
            nightSkyboxMaterial.mainTexture = instance.m_SkyboxMaterial.GetTexture("_Night");
        }
    }
}
