using System.Collections;
using GameFramework.BodySystems;
using GameFramework.CollisionSystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.Kinematics;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UniRx;
using UnityEngine;

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

        protected override string SceneAssetPath => "battle";

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);
            
            var taskRunner = Services.Get<TaskRunner>();
            var cameraController = Services.Get<CameraController>();

            // BattleModelの生成
            var battleModel = BattleModel.Create();
            battleModel.RegisterTask(TaskOrder.Logic);
            yield return battleModel.SetupAsync()
                .StartAsEnumerator(scope);

            // BodyManagerの生成
            var bodyManager = new BodyManager(new BodyBuilder());
            ServiceContainer.Set(bodyManager);
            bodyManager.RegisterTask(TaskOrder.Body);
            
            // CollisionManagerの登録
            var collisionManager = new CollisionManager();
            ServiceContainer.Set(collisionManager);
            collisionManager.RegisterTask(TaskOrder.Collision);

            // タスク登録
            Services.Get<CameraController>().RegisterTask(TaskOrder.Camera);
            Services.Get<BattleInput>().RegisterTask(TaskOrder.Input);

            // RootAngle作成
            _rootAngle = new GameObject("RootAngle").transform;
            var rootAngleConstraint = cameraController.GetAttachment<ParentAttachment>("RootAngle");
            rootAngleConstraint.Sources = new[] {
                new ConstraintResolver.TargetSource {
                    target = _rootAngle,
                    weight = 1.0f
                }
            };

            // PlayerEntityの生成
            _playerEntity = new Entity();
            _playerEntity.SetupPlayerAsync(battleModel.PlayerModel, scope)
                .Subscribe(entity => {
                    // CameraのAttachment設定
                    var playerConstraint = cameraController.GetAttachment<ParentAttachment>("Player");
                    playerConstraint.Sources = new[] {
                        new ConstraintResolver.TargetSource {
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

            // todo:コリジョンテスト
            if (Input.GetKeyDown(KeyCode.C)) {
                var collisionManager = Services.Get<CollisionManager>();
                var handle = collisionManager.Register(new SphereCollision(Vector3.forward * 5, 10), -1, result => {
                    Debug.Log($"Hit:{result.collider.name}");
                });
                
                Observable.TimerFrame(50)
                    .Subscribe(_ => handle.Dispose());
            }
            if (Input.GetKeyDown(KeyCode.V)) {
                var collisionManager = Services.Get<CollisionManager>();
                var handle = collisionManager.Register(new BoxCollision(Vector3.back * 5, Vector3.one * 10, Quaternion.identity), -1, result => {
                    Debug.Log($"Hit:{result.collider.name}");
                });
                
                Observable.TimerFrame(50)
                    .Subscribe(_ => handle.Dispose());
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