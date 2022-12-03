using System.Collections;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.Kinematics;
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
        // 生成したPlayerのId
        private int _playerId;
        // 生成したPlayerのEntity
        private Entity _playerEntity;
        
        public override string SceneAssetPath => "battle";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
            
            var taskRunner = Services.Get<TaskRunner>();
            
            // todo:Battleモデルの生成 > ここでPlayerModelを作る
            
            // BodyManagerの生成
            var bodyManager = new BodyManager();
            taskRunner.Register(bodyManager, TaskOrder.Body);
            ServiceContainer.Set(bodyManager);
            
            // PlayerEntityの生成
            var playerEntity = new Entity();
            var playerModel = BattlePlayerModel.Create();
            _playerId = playerModel.Id;
            playerModel.Update("HogeMan", "pl001", 100);
            yield return playerEntity.SetupPlayerAsync(playerModel)
                .StartAsEnumerator(scope);
            _playerEntity = playerEntity;
            
            // 子シチュエーションコンテナの生成
            _situationContainer = new SituationContainer(this);
            //_situationContainer.Transition()
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void SetupInternal(TransitionHandle handle, IScope scope) {
            base.SetupInternal(handle, scope);

            // CameraのConstraint設定
            var cameraController = Services.Get<CameraController>();
            var playerConstraint = cameraController.GetConstraint<ParentConstraint>("Player");
            playerConstraint.Sources = new[] {
                new Constraint.TargetSource {
                    target = _playerEntity.GetBody().Transform,
                    weight = 1.0f
                }
            };
            
            // Cameraのタスク登録
            var taskRunner = Services.Get<TaskRunner>();
            taskRunner.Register(cameraController, TaskOrder.Camera);
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

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="handle"></param>
        protected override void CleanupInternal(TransitionHandle handle) {
            var taskRunner = Services.Get<TaskRunner>();
            
            // Cameraのタスク登録解除
            var cameraController = Services.Get<CameraController>();
            taskRunner.Unregister(cameraController);
            
            base.CleanupInternal(handle);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        protected override void UnloadInternal(TransitionHandle handle) {
            _playerEntity.Dispose();
            
            // todo:ここでBattleModelを解放
            BattlePlayerModel.Delete(_playerId);
            
            base.UnloadInternal(handle);
        }
    }
}
