using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// ゲームシーン用のSituation
    /// </summary>
    public class GameSceneSituation : SceneSituation {
        // GameScene内シチュエーション用コンテナ
        private SituationContainer _situationContainer;
        
        public override string SceneAssetPath => "game";

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void SetupInternal(TransitionHandle handle, IScope scope) {
            var taskRunner = Services.Get<TaskRunner>();
            
            // Gameモデルの生成
            
            // BodyManagerの生成
            var bodyManager = new BodyManager();
            taskRunner.Register(bodyManager, TaskOrder.Body);
            
            // PlayerEntityの生成
            
            // 子シチュエーションコンテナの生成
            _situationContainer = new SituationContainer();
            //_situationContainer.Transition()
        }

        protected override void UpdateInternal() {
            Debug.Log("Test");
        }
    }
}
