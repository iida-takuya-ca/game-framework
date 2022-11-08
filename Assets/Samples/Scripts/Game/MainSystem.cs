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
        protected override IEnumerator RebootRoutineInternal() {
            yield break;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator StartRoutineInternal(object[] args) {
            // 各種システム初期化
            _taskRunner = new TaskRunner();
            _sceneSituationContainer = new SceneSituationContainer();
            Services.Instance.Set(_taskRunner);
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