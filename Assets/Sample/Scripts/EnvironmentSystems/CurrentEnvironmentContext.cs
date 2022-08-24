using UnityEngine;
using UnityEngine.Rendering;

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

    /// <summary>
    /// 値の反映
    /// </summary>
    void GameFramework.EnvironmentSystems.IEnvironmentContext.Apply() {
        this.Apply();
    }

    /// <summary>
    /// 値の線形補間
    /// </summary>
    GameFramework.EnvironmentSystems.IEnvironmentContext GameFramework.EnvironmentSystems.IEnvironmentContext.Lerp(GameFramework.EnvironmentSystems.IEnvironmentContext target, float rate) {
        return this.Lerp((IEnvironmentContext)target, rate);
    }
}
