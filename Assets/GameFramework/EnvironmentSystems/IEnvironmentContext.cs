using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// 環境設定インターフェース
    /// </summary>
    public interface IEnvironmentContext {
        Material Skybox { get; }
        AmbientMode AmbientMode { get; }
        float AmbientIntensity { get; }
        Color AmbientSkyColor { get; }
        Color AmbientEquatorColor { get; }
        Color AmbientGroundColor { get; }
        Color AmbientLight { get; }
        SphericalHarmonicsL2 AmbientProbe { get; }
    }
}
