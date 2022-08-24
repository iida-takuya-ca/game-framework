using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 環境設定用のアセット
/// </summary>
public class EnvironmentContextData : ScriptableObject, IEnvironmentContext {
    [SerializeField] private Material _skybox;
    [SerializeField] private AmbientMode _ambientMode;
    [SerializeField] private float _ambientIntensity;
    [SerializeField] private Color _ambientSkyColor;
    [SerializeField] private Color _ambientEquatorColor;
    [SerializeField] private Color _ambientGroundColor;
    [SerializeField] private Color _ambientLight;

    public Material Skybox => _skybox;
    public AmbientMode AmbientMode => _ambientMode;
    public float AmbientIntensity => _ambientIntensity;
    public Color AmbientSkyColor => _ambientSkyColor;
    public Color AmbientEquatorColor => _ambientEquatorColor;
    public Color AmbientGroundColor => _ambientGroundColor;
    public Color AmbientLight => _ambientLight;
    public SphericalHarmonicsL2 AmbientProbe => RenderSettings.ambientProbe;

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