using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// 現在の環境設定
    /// </summary>
    public class CurrentEnvironmentContext : IEnvironmentContext {
        public virtual Material Skybox => RenderSettings.skybox;
        public virtual AmbientMode AmbientMode => RenderSettings.ambientMode;
        public virtual float AmbientIntensity => RenderSettings.ambientIntensity;
        public virtual Color AmbientSkyColor => RenderSettings.ambientSkyColor;
        public virtual Color AmbientEquatorColor => RenderSettings.ambientEquatorColor;
        public virtual Color AmbientGroundColor => RenderSettings.ambientGroundColor;
        public virtual Color AmbientLight => RenderSettings.ambientLight;
        public virtual SphericalHarmonicsL2 AmbientProbe => RenderSettings.ambientProbe;
    }
}
