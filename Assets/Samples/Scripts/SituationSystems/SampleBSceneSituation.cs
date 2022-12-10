using System.Collections;
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
    /// 読み込み処理
    /// </summary>
    protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
        yield return base.LoadRoutineInternal(handle, scope);
        Debug.Log($"[{Time.frameCount}]Begin Load:{GetType().Name}");
        yield return new WaitForSeconds(1.0f);
        Debug.Log($"[{Time.frameCount}]End Load:{GetType().Name}");
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void SetupInternal(TransitionHandle handle, IScope scope) {
        base.SetupInternal(handle, scope);
        Debug.Log($"[{Time.frameCount}]Setup:{GetType().Name} Back > {handle.Back}");
    }

    /// <summary>
    /// 開く処理
    /// </summary>
    protected override IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
        yield return base.OpenRoutineInternal(handle, animationScope);
        Debug.Log($"[{Time.frameCount}]Begin Open:{GetType().Name}");
        yield return new WaitForSeconds(1.0f);
        Debug.Log($"[{Time.frameCount}]End Open:{GetType().Name}");
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void UpdateInternal() {
        base.UpdateInternal();

        if (Input.GetKeyDown(KeyCode.Space)) {
            ParentContainer.Transition(new SampleASceneSituation());
        }
    }

    /// <summary>
    /// 閉じる処理
    /// </summary>
    protected override IEnumerator CloseRoutineInternal(TransitionHandle handle, IScope animationScope) {
        Debug.Log($"[{Time.frameCount}]Begin Close:{GetType().Name}");
        yield return new WaitForSeconds(1.0f);
        Debug.Log($"[{Time.frameCount}]End Close:{GetType().Name}");
        yield return base.CloseRoutineInternal(handle, animationScope);
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    protected override void CleanupInternal(TransitionHandle handle) {
        Debug.Log($"[{Time.frameCount}]Cleanup:{GetType().Name}");
        base.CleanupInternal(handle);
    }

    /// <summary>
    /// 解放処理
    /// </summary>
    protected override void UnloadInternal(TransitionHandle handle) {
        Debug.Log($"[{Time.frameCount}]Unload:{GetType().Name}");
        base.UnloadInternal(handle);
    }
}