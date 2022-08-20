using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

/// <summary>
/// シーン遷移に使うシチュエーション
/// </summary>
public class SampleBSceneSituation : SceneSituation {
    // シーンのアセットパス
    public override string SceneAssetPath => "sample_b";

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void SetupInternal(TransitionHandle handle, IScope scope) {
        base.SetupInternal(handle, scope);
        
        Debug.Log($"Setup {nameof(SampleBSceneSituation)} Back:{handle.Back}");
        Debug.Log($"Canvas:{ServiceLocator.Get<SampleCanvas>()}");
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void UpdateInternal() {
        base.UpdateInternal();
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            Parent.Transition(new SampleASceneSituation());
        }
    }
}