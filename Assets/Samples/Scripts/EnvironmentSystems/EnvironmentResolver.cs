using GameFramework.EnvironmentSystems;
using UnityEngine;

/// <summary>
/// 環境制御用クラス
/// </summary>
public class EnvironmentResolver : EnvironmentResolver<IEnvironmentContext> {
    /// <summary>
    /// 現在反映されている値を格納したContextを取得
    /// </summary>
    protected override IEnvironmentContext GetCurrentInternal() {
        return new CurrentEnvironmentContext();
    }
    
    /// <summary>
    /// 値の反映
    /// </summary>
    /// <param name="context">反映対象のコンテキスト</param>
    protected override void ApplyInternal(IEnvironmentContext context) {
        RenderSettings.skybox = context.Skybox;
        RenderSettings.ambientMode = context.AmbientMode;

        RenderSettings.ambientIntensity = context.AmbientIntensity;
        RenderSettings.ambientSkyColor = context.AmbientSkyColor;
        RenderSettings.ambientEquatorColor = context.AmbientEquatorColor;
        RenderSettings.ambientGroundColor = context.AmbientGroundColor;
        RenderSettings.ambientLight = context.AmbientLight;
        RenderSettings.ambientProbe = context.AmbientProbe;
    }

    /// <summary>
    /// 値の線形補間
    /// </summary>
    /// <param name="current">補間元</param>
    /// <param name="target">補間先</param>
    /// <param name="rate">ブレンド率</param>
    protected override IEnvironmentContext LerpInternal(IEnvironmentContext current, IEnvironmentContext target, float rate) {
        rate = Mathf.Clamp01(rate);

        var result = new EnvironmentContext();

        // ブレンド不可能な値を適用
        result.Skybox = target.Skybox;
        result.AmbientMode = target.AmbientMode;

        // ブレンド可能な値を適用
        result.AmbientIntensity = Mathf.Lerp(current.AmbientIntensity, target.AmbientIntensity, rate);
        result.AmbientSkyColor = Color.Lerp(current.AmbientSkyColor, target.AmbientSkyColor, rate);
        result.AmbientEquatorColor = Color.Lerp(current.AmbientEquatorColor, target.AmbientEquatorColor, rate);
        result.AmbientGroundColor = Color.Lerp(current.AmbientGroundColor, target.AmbientGroundColor, rate);
        result.AmbientLight = Color.Lerp(current.AmbientLight, target.AmbientLight, rate);
        result.AmbientProbe = current.AmbientProbe * (1.0f - rate) + target.AmbientProbe * rate;

        return result;
    }
}