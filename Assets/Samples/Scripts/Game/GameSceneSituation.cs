using GameFramework.Core;
using GameFramework.SituationSystems;
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
            // Gameモデルの生成
            
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
