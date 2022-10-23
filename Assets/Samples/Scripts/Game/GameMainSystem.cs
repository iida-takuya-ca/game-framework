using System.Collections;
using GameFramework.Core;
using GameFramework.TaskSystems;
using SampleGame;

public class GameMainSystem : MainSystem<GameMainSystem> {
    private TaskRunner _taskRunner;
    private SceneSituationContainer _sceneSituationContainer;
    
    /// <summary>
    /// リブート処理
    /// </summary>
    protected override IEnumerator RebootRoutineInternal() {
        yield break;
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override IEnumerator StartRoutineInternal() {
        // 各種システム初期化
        _taskRunner = new TaskRunner();
        _sceneSituationContainer = new SceneSituationContainer();
        Services.Instance.Set(_taskRunner);
        _taskRunner.Register(_sceneSituationContainer);
        
        // ゲームシーンの読み込み
        var handle = _sceneSituationContainer.Transition(new GameSceneSituation());
        yield return handle;
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void UpdateInternal() {
        _taskRunner.Update();
    }

    /// <summary>
    /// 後更新処理
    /// </summary>
    protected override void LateUpdateInternal() {
        _taskRunner.LateUpdate();
    }

    /// <summary>
    /// 廃棄時処理
    /// </summary>
    protected override void OnDestroyInternal() {
        _sceneSituationContainer.Dispose();
        _taskRunner.Dispose();
    }
}