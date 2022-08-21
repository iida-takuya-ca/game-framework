using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 環境設定
/// </summary>
public class EnvironmentContext : IEnvironmentContext {
    public Material Skybox { get; set; } = null;
    public AmbientMode AmbientMode { get; set; } = AmbientMode.Custom;
    public float AmbientIntensity { get; set; } = 1.0f;
    public Color AmbientSkyColor { get; set; } = Color.black;
    public Color AmbientEquatorColor { get; set; } = Color.black;
    public Color AmbientGroundColor { get; set; } = Color.black;
    public Color AmbientLight { get; set; } = Color.black;
    public SphericalHarmonicsL2 AmbientProbe { get; set; } = new SphericalHarmonicsL2();

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
