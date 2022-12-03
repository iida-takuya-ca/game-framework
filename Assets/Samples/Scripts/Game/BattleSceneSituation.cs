using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.Kinematics;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UniRx;
using UnityEngine;
using UnityEngine.Animations;
using ParentConstraint = GameFramework.Kinematics.ParentConstraint;

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
        // カメラ方向を使ったRoot位置
        private Transform _rootAngle;
        
        public override string SceneAssetPath => "battle";

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void SetupInternal(TransitionHandle handle, IScope scope) {
            base.SetupInternal(handle, scope);
            
            var taskRunner = Services.Get<TaskRunner>();
            var cameraController = Services.Get<CameraController>();
            
            // BodyManagerの生成
            var bodyManager = new BodyManager();
            ServiceContainer.Set(bodyManager);
            taskRunner.Register(bodyManager, TaskOrder.Body);
            
            // Cameraのタスク登録
            taskRunner.Register(Services.Get<CameraController>(), TaskOrder.Camera);
            
            // Inputのタスク登録
            taskRunner.Register(Services.Get<BattleInput>(), TaskOrder.Input);
            
            // todo:Battleモデルの生成 > ここでPlayerModelを作る
            var playerModel = BattlePlayerModel.Create();
            playerModel.Update("HogeMan", "pl001", 100);
            _playerId = playerModel.Id;
            
            // RootAngle作成
            _rootAngle = new GameObject("RootAngle").transform;
            var rootAngleConstraint = cameraController.GetConstraint<ParentConstraint>("RootAngle");
            rootAngleConstraint.Sources = new[] {
                new Constraint.TargetSource {
                    target = _rootAngle,
                    weight = 1.0f
                }
            };
            
            // PlayerEntityの生成
            _playerEntity = new Entity();
            _playerEntity.SetupPlayerAsync(playerModel)
                .Subscribe(entity => {
                    // CameraのConstraint設定
                    var playerConstraint = cameraController.GetConstraint<ParentConstraint>("Player");
                    playerConstraint.Sources = new[] {
                        new Constraint.TargetSource {
                            target = entity.GetBody().Transform,
                            weight = 1.0f
                        }
                    };
                })
                .ScopeTo(scope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            // todo:取り合えずここでRootAngle更新
            var playerBody = _playerEntity.GetBody();
            if (playerBody != null) {
                _rootAngle.position = playerBody.Transform.position;
            }

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
            
            // PlayerEntity削除
            _playerEntity.Dispose();
            
            // todo:ここでBattleModelを解放
            BattlePlayerModel.Delete(_playerId);
            
            // Cameraのタスク登録解除
            var cameraController = Services.Get<CameraController>();
            taskRunner.Unregister(cameraController);
            
            // Inputのタスク登録解除
            var input = Services.Get<BattleInput>();
            taskRunner.Unregister(input);
            
            base.CleanupInternal(handle);
        }
    }
}
