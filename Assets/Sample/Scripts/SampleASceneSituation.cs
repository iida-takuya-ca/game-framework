using GameFramework.Core;
using GameFramework.SituationSystems;
using UniRx;
using UnityEngine;

/// <summary>
/// シーン遷移に使うシチュエーション
/// </summary>
public class SampleASceneSituation : SceneSituation {
    // シーンのアセットパス
    public override string SceneAssetPath => "sample_a";

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void SetupInternal(TransitionHandle handle, IScope scope) {
        base.SetupInternal(handle, scope);

        // 100フレームおきにログ出力
        Observable.IntervalFrame(100)
            .TakeUntil(scope)
            .Subscribe(x => {
                Debug.Log($"Frame:{(x + 1) * 100}");
            });
        
        Debug.Log($"Setup {nameof(SampleASceneSituation)} Back:{handle.Back}");
        Debug.Log($"Canvas:{ServiceLocator.Get<SampleCanvas>()}");
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void UpdateInternal() {
        base.UpdateInternal();
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            ParentContainer.Transition(new SampleBSceneSituation());
        }
    }
}