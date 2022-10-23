using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 環境設定
/// </summary>
public interface IEnvironmentContext : GameFramework.EnvironmentSystems.IEnvironmentContext {
    Material Skybox { get; }
    AmbientMode AmbientMode { get; }
    float AmbientIntensity { get; }
    Color AmbientSkyColor { get; }
    Color AmbientEquatorColor { get; }
    Color AmbientGroundColor { get; }
    Color AmbientLight { get; }
    SphericalHarmonicsL2 AmbientProbe { get; }
}
