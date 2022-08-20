using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// シーンのRootとなるシチュエーション
/// </summary>
public class SceneRootSituation : Situation, ILateUpdatableTask {
    // アクティブか
    bool ITask.IsActive => true;
    
    /// <summary>
    /// シーン遷移用クラス
    /// </summary>
    private class SceneTransition : OutInTransition {
        /// <summary>
        /// 初期化前処理
        /// </summary>
        protected override IEnumerator PreInitializeRoutine(TransitionInfo transitionInfo) {
            var next = transitionInfo.next;
            if (next == null) {
                yield break;
            }
            
            // シーンの切り替え
            yield return SceneManager.LoadSceneAsync(((SceneSituation)next).SceneAssetPath, LoadSceneMode.Single);
        }
    }

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
    /// 初期化処理
    /// </summary>
    protected override void SetupInternal(TransitionHandle handle, IScope scope) {
        Transition(new SampleASceneSituation());
    }

    /// <summary>
    /// 遷移用のTransition取得
    /// </summary>
    protected override ITransition GetDefaultTransition() {
        return new SceneTransition();
    }

    /// <summary>
    /// 遷移を行えるか
    /// </summary>
    protected override bool CheckTransitionInternal(Situation next, ITransition transition) {
        return next is SceneSituation && transition is SceneTransition;
    }
}