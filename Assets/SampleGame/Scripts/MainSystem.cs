using System.Collections;
using GameFramework.AssetSystems;
using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SampleGame {
    /// <summary>
    /// SampleGameのメインシステム（実行中常に存在するインスタンス）
    /// </summary>
    public class MainSystem : MainSystem<MainSystem> {
        [SerializeField]
        private ServiceContainerInstaller _globalObject;

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
            // FPS設定
            Application.targetFrameRate = 30;
            
            // GlobalObjectを初期化
            DontDestroyOnLoad(_globalObject.gameObject);
            // RootのServiceにインスタンスを登録
            _globalObject.Install(Services.Instance);
            
            // 各種システム初期化
            _taskRunner = new TaskRunner();
            Services.Instance.Set(_taskRunner);
            var assetManager = new AssetManager();
            assetManager.Initialize(
                new AssetDatabaseAssetProvider(),
                new ResourcesAssetProvider());
            Services.Instance.Set(assetManager);
            _sceneSituationContainer = new SceneSituationContainer();
            _taskRunner.Register(_sceneSituationContainer);
            var environmentManager = new EnvironmentManager(new EnvironmentResolver());
            _taskRunner.Register(environmentManager, TaskOrder.PostSystem);
            Services.Instance.Set(environmentManager);

            // 各種GlobalObjectのタスク登録
            _taskRunner.Register(Services.Get<FadeController>(), TaskOrder.UI);

            // 開始用シーンの読み込み
            var startSituation = default(SceneSituation);
            if (args.Length > 0) {
                startSituation = args[0] as SceneSituation;
            }

            if (startSituation == null) {
                // 未指定ならLogin > Titleへ
                startSituation = new LoginSceneSituation(new TitleSceneSituation());
            }
            
            // Addressables初期化
            yield return Addressables.InitializeAsync();

            // 初期シチュエーションに遷移
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
            Services.Instance.Clear();
        }

        /// <summary>
        /// アプリケーション終了時の処理
        /// </summary>
        private void OnApplicationQuit() {
            Services.Instance.Clear();
        }
    }
}