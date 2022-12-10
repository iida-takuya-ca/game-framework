using GameFramework.TaskSystems;
using UnityEngine;

/// <summary>
/// シチュエーションサンプル
/// </summary>
public class SituationSample : MonoBehaviour {
    // Rootシチュエーション
    private SceneSituationContainer _sceneSituationContainer;
    // Task実行用
    private TaskRunner _taskRunner;

    /// <summary>
    /// 生成時処理
    /// </summary>
    private void Awake() {
        DontDestroyOnLoad(this);

        _taskRunner = new TaskRunner();
        _sceneSituationContainer = new SceneSituationContainer();
        _taskRunner.Register(_sceneSituationContainer);
    }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        _sceneSituationContainer.Transition(new SampleASceneSituation());
    }

    /// <summary>
    /// 廃棄処理
    /// </summary>
    private void OnDestroy() {
        _taskRunner.Dispose();
        _sceneSituationContainer.Dispose();
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