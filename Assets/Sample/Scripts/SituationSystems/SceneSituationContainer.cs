using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を行えるシチュエーションコンテナ
/// </summary>
public class SceneSituationContainer : SituationContainer, ILateUpdatableTask {
    // アクティブか
    bool ITask.IsActive => true;
    
    /// <summary>
    /// タスク更新
    /// </summary>
    void ITask.Update() {
        // Rootシーン更新
        Update();
    }

    /// <summary>
    /// タスク後更新
    /// </summary>
    void ILateUpdatableTask.LateUpdate() {
        // Rootシーン更新
        LateUpdate();
    }

    /// <summary>
    /// 遷移用のTransition取得
    /// </summary>
    protected override ITransition GetDefaultTransition() {
        return new OutInTransition();
    }

    /// <summary>
    /// 遷移を行えるか
    /// </summary>
    protected override bool CheckTransitionInternal(Situation next, ITransition transition) {
        return next is SceneSituation && transition is OutInTransition;
    }

    /// <summary>
    /// 読み込みの直前コルーチン
    /// </summary>
    /// <param name="handle">遷移ハンドル</param>
    protected override IEnumerator PreLoadNextRoutine(TransitionHandle handle) {
        var next = handle.Next as SceneSituation;
        if (next == null) {
            yield break;
        }
        // シーンの切り替え
        yield return SceneManager.LoadSceneAsync(next.SceneAssetPath, LoadSceneMode.Single);
    }
}