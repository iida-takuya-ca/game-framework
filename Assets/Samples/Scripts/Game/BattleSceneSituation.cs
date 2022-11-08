using System.Collections;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// Battle用のSituation
    /// </summary>
    public class BattleSceneSituation : SceneSituation {
        // BattleScene内シチュエーション用コンテナ
        private SituationContainer _situationContainer;
        
        public override string SceneAssetPath => "battle";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
            
            var taskRunner = Services.Get<TaskRunner>();
            
            // Gameモデルの生成
            
            // BodyManagerの生成
            var bodyManager = new BodyManager();
            taskRunner.Register(bodyManager, TaskOrder.Body);
            ServiceContainer.Set(bodyManager);
            
            // PlayerEntityの生成
            var playerEntity = new Entity();
            yield return playerEntity.SetupPlayerAsync("pl001")
                .StartAsEnumerator(scope);
            
            // 子シチュエーションコンテナの生成
            _situationContainer = new SituationContainer(this);
            //_situationContainer.Transition()
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.Space)) {
                MainSystem.Instance.Reboot(new BattleSceneSituation());
            }
        }
    }
}
