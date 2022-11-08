using System.Collections;
using GameFramework.Core;
using GameFramework.TaskSystems;

namespace SampleGame {
    /// <summary>
    /// SampleGameのメインシステム（実行中常に存在するインスタンス）
    /// </summary>
    public class MainSystem : MainSystem<MainSystem> {
        private TaskRunner _taskRunner;
        private SceneSituationContainer _sceneSituationContainer;

        /// <summary>
        /// リブート処理
        /// </summary>
        /// <param name="args">リブート時に渡された引数</param>
        protected override IEnumerator RebootRoutineInternal(object[] args) {
            // Scene用のContainerの作成しなおし
            _sceneSituationContainer.Dispose();
            _taskRunner.Unregister(_sceneSituationContainer);
            _sceneSituationContainer = new SceneSituationContainer();
            _taskRunner.Register(_sceneSituationContainer);
            
            // 開始用シーンの読み込み
            var startSituation = default(SceneSituation);
            if (args.Length > 0) {
                startSituation = args[0] as SceneSituation;
            }
            if (startSituation == null) {
                // 未指定ならLogin > Titleへ
                startSituation = new LoginSceneSituation(new TitleSceneSituation());
            }
            
            var handle = _sceneSituationContainer.Transition(startSituation);
            yield return handle;   
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator StartRoutineInternal(object[] args) {
            // 各種システム初期化
            _taskRunner = new TaskRunner();
            Services.Instance.Set(_taskRunner);
            _sceneSituationContainer = new SceneSituationContainer();
            _taskRunner.Register(_sceneSituationContainer);

            // 開始用シーンの読み込み
            var startSituation = default(SceneSituation);
            if (args.Length > 0) {
                startSituation = args[0] as SceneSituation;
            }
            if (startSituation == null) {
                // 未指定ならLogin > Titleへ
                startSituation = new LoginSceneSituation(new TitleSceneSituation());
            }
            var handle = _sceneSituationContainer.Transition(startSituation);
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
}