using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// Unityにある環境設定
    /// </summary>
    [Serializable]
    public struct EnvironmentDefaultSettings {
        public Light sun;
        public Color subtractiveShadowColor;
        public AmbientMode ambientMode;
        public Color ambientSkyColor;
        public Color ambientEquatorColor;
        public Color ambientGroundColor;
        public float ambientIntensity;
        public Material skyboxMaterial;
        public DefaultReflectionMode defaultReflectionMode;
        public int defaultReflectionResolution;
        public Texture customReflection;
        public float reflectionIntensity;
        public int reflectionBounces;

        /// <summary>
        /// 現在の値を取得
        /// </summary>
        /// <returns></returns>
        public static EnvironmentDefaultSettings GetCurrent() {
            return new EnvironmentDefaultSettings {
                sun = RenderSettings.sun,
                subtractiveShadowColor = RenderSettings.subtractiveShadowColor,
                ambientMode = RenderSettings.ambientMode,
                ambientSkyColor = RenderSettings.ambientSkyColor,
                ambientEquatorColor = RenderSettings.ambientEquatorColor,
                ambientGroundColor = RenderSettings.ambientGroundColor,
                ambientIntensity = RenderSettings.ambientIntensity,
                skyboxMaterial = RenderSettings.skybox,
                defaultReflectionMode = RenderSettings.defaultReflectionMode,
                defaultReflectionResolution = RenderSettings.defaultReflectionResolution,
                customReflection = RenderSettings.customReflection,
                reflectionIntensity = RenderSettings.reflectionIntensity,
                reflectionBounces = RenderSettings.reflectionBounces
            };
        }

        /// <summary>
        /// 値の反映
        /// </summary>
        public void Apply() {
            RenderSettings.sun = sun;
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.skybox = skyboxMaterial;
            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
            RenderSettings.customReflection = customReflection;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.reflectionBounces = reflectionBounces;
        }

        /// <summary>
        /// 値の補間（補間出来ない物はtargetを使用）
        /// </summary>
        /// <param name="target">補間対象</param>
        /// <param name="ratio">ブレンド率</param>
        public EnvironmentDefaultSettings Lerp(EnvironmentDefaultSettings target, float ratio) {
            var newSettings = target;
            newSettings.subtractiveShadowColor = Color.Lerp(subtractiveShadowColor, target.subtractiveShadowColor, ratio);
            newSettings.ambientSkyColor = Color.Lerp(ambientSkyColor, target.ambientSkyColor, ratio);
            newSettings.ambientEquatorColor = Color.Lerp(ambientEquatorColor, target.ambientEquatorColor, ratio);
            newSettings.ambientGroundColor = Color.Lerp(ambientGroundColor, target.ambientGroundColor, ratio);
            newSettings.ambientIntensity = Mathf.Lerp(ambientIntensity, target.ambientIntensity, ratio);
            return newSettings;
        }
    }
}
