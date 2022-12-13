using System.Collections;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.Kinematics;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UniRx;
using UnityEngine;
using ParentConstraint = GameFramework.Kinematics.ParentConstraint;

namespace SampleGame {
    /// <summary>
    /// Battle用のSituation
    /// </summary>
    public class BattleSceneSituation : SceneSituation {
        /// <summary>
        /// Body生成クラス
        /// </summary>
        private class BodyBuilder : IBodyBuilder {
            /// <summary>
            /// 構築処理
            /// </summary>
            public void Build(IBody body, GameObject gameObject) {
                RequireComponent<StatusEventListener>(gameObject);
            }

            private void RequireComponent<T>(GameObject gameObject)
                where T : Component {
                var component = gameObject.GetComponent<T>();
                if (component == null) {
                    gameObject.AddComponent<T>();
                }
            }
        }

        // BattleScene内シチュエーション用コンテナ
        private SituationContainer _situationContainer;
        // 生成したPlayerのEntity
        private Entity _playerEntity;
        // カメラ方向を使ったRoot位置
        private Transform _rootAngle;

        public override string SceneAssetPath => "battle";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);

            // BattleModelの生成
            var battleModel = BattleModel.Create();
            battleModel.RegisterTask(TaskOrder.Logic);
            yield return battleModel.SetupAsync()
                .StartAsEnumerator(scope);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void SetupInternal(TransitionHandle handle, IScope scope) {
            base.SetupInternal(handle, scope);

            var taskRunner = Services.Get<TaskRunner>();
            var cameraController = Services.Get<CameraController>();
            var battleModel = BattleModel.Get();

            // BodyManagerの生成
            var bodyManager = new BodyManager(new BodyBuilder());
            ServiceContainer.Set(bodyManager);
            taskRunner.Register(bodyManager, TaskOrder.Body);

            // タスク登録
            Services.Get<CameraController>().RegisterTask(TaskOrder.Camera);
            Services.Get<BattleInput>().RegisterTask(TaskOrder.Input);

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
            _playerEntity.SetupPlayerAsync(battleModel.PlayerModel, scope)
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
            // PlayerEntity削除
            _playerEntity.Dispose();

            // Cameraのタスク登録解除
            var cameraController = Services.Get<CameraController>();
            cameraController.UnregisterTask();

            // Inputのタスク登録解除
            var input = Services.Get<BattleInput>();
            input.UnregisterTask();

            base.CleanupInternal(handle);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        protected override void UnloadInternal(TransitionHandle handle) {
            // BattleModel削除
            var battleModel = BattleModel.Get();
            battleModel.UnregisterTask();
            BattleModel.Delete();

            base.UnloadInternal(handle);
        }
    }
}