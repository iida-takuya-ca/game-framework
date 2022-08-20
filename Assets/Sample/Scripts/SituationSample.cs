using System;
using System.Collections;
using GameFramework.TaskSystems;
using UnityEngine;

/// <summary>
/// シチュエーションサンプル
/// </summary>
public class SituationSample : MonoBehaviour {
    // Rootシチュエーション
    private SceneRootSituation _sceneRootSituation;
    // Task実行用
    private TaskRunner _taskRunner;
    
    /// <summary>
    /// 生成時処理
    /// </summary>
    private void Awake() {
        DontDestroyOnLoad(this);

        _taskRunner = new TaskRunner();
        _sceneRootSituation = new SceneRootSituation();
        _taskRunner.Register(_sceneRootSituation);
    }

    /// <summary>
    /// 廃棄処理
    /// </summary>
    private void OnDestroy() {
        _sceneRootSituation.CleanupRoot();
        _taskRunner.Dispose();
    }

    /// <summary>
    /// 開始処理
    /// </summary>
    private IEnumerator Start() {
        // Rootとして初期化
        var handle = _sceneRootSituation.SetupRoot();
        while (!handle.IsDone) {
            yield return null;
        }
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update() {
        _taskRunner.Update();
    }

    /// <summary>
    /// 後更新処理
    /// </summary>
    private void LateUpdate() {
        _taskRunner.LateUpdate();
    }
}