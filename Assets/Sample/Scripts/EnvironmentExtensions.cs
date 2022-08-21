using UnityEngine;

/// <summary>
/// 環境システム用拡張メソッド
/// </summary>
public static class EnvironmentExtensions {
    /// <summary>
    /// EnvironmentContextの反映
    /// </summary>
    /// <param name="source">自身</param>
    public static void Apply(this IEnvironmentContext source) {
        RenderSettings.skybox = source.Skybox;
        RenderSettings.ambientMode = source.AmbientMode;

        RenderSettings.ambientIntensity = source.AmbientIntensity;
        RenderSettings.ambientSkyColor = source.AmbientSkyColor;
        RenderSettings.ambientEquatorColor = source.AmbientEquatorColor;
        RenderSettings.ambientGroundColor = source.AmbientGroundColor;
        RenderSettings.ambientLight = source.AmbientLight;
        RenderSettings.ambientProbe = source.AmbientProbe;
    }

    /// <summary>
    /// EnvironmentContextの値をブレンド
    /// </summary>
    /// <param name="source">自身</param>
    /// <param name="target">相手</param>
    /// <param name="rate">ブレンド率</param>
    public static IEnvironmentContext Lerp(this IEnvironmentContext source, IEnvironmentContext target, float rate) {
        rate = Mathf.Clamp01(rate);

        var result = new EnvironmentContext();

        // ブレンド不可能な値を適用
        result.Skybox = target.Skybox;
        result.AmbientMode = target.AmbientMode;

        // ブレンド可能な値を適用
        result.AmbientIntensity = Mathf.Lerp(source.AmbientIntensity, target.AmbientIntensity, rate);
        result.AmbientSkyColor = Color.Lerp(source.AmbientSkyColor, target.AmbientSkyColor, rate);
        result.AmbientEquatorColor = Color.Lerp(source.AmbientEquatorColor, target.AmbientEquatorColor, rate);
        result.AmbientGroundColor = Color.Lerp(source.AmbientGroundColor, target.AmbientGroundColor, rate);
        result.AmbientLight = Color.Lerp(source.AmbientLight, target.AmbientLight, rate);
        result.AmbientProbe = source.AmbientProbe * (1.0f - rate) + target.AmbientProbe * rate;

        return result;
    }
}