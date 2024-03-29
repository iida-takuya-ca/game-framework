using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.CollisionSystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using GameFramework.CutsceneSystems;
using GameFramework.ProjectileSystems;
using GameFramework.SituationSystems;
using GameFramework.VfxSystems;
using UniRx;
using UnityEngine;
using SampleGame.Battle;
using UnityEngine.SceneManagement;
using Component = UnityEngine.Component;

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
        private ActorEntity _playerActorEntity;
        
        // テスト用カットシーン
        private Scene _cutsceneScene;

        private LayeredTime _layeredTime;

        protected override string SceneAssetPath => "battle";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);

            var ct = scope.ToCancellationToken();
            
            // フィールド読み込み
            yield return new FieldSceneAssetRequest("fld001")
                .LoadAsync(true, scope, ct)
                .ToCoroutine();

            // カットシーン読み込み
            yield return new BattleEventCutsceneAssetRequest("001")
                .LoadAsync(true, scope, ct)
                .ContinueWith(x => {
                    _cutsceneScene = x;
                    foreach (var obj in x.GetRootGameObjects()) {
                        obj.SetActive(false);
                    }
                })
                .ToCoroutine();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            var ct = scope.ToCancellationToken();

            // BattleModelの生成
            var battleModel = BattleModel.Create()
                .ScopeTo(scope);
            
            // 読み込み処理
            var playerMaster = default(BattlePlayerMasterData);
            var playerActorSetup = default(PlayerActorSetupData);
            var playerActionDataList = default(PlayerActorActionData[]);
            var uniTask = LoadPlayerMasterAsync("pl001", scope, ct)
                .ContinueWith(tuple => {
                    playerMaster = tuple.Item1;
                    playerActorSetup = tuple.Item2;
                    playerActionDataList = tuple.Item3;
                });
            yield return uniTask.ToCoroutine();
            
            // PlayerModelの初期化
            battleModel.PlayerModel.Update(playerMaster.name, playerMaster.assetKey, playerMaster.healthMax);
            battleModel.PlayerModel.ActorModel.Update(playerActorSetup, playerActionDataList);
            
            // CameraManagerの初期化
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
            
            // CutsceneManagerの生成
            var cutsceneManager = new CutsceneManager();
            ServiceContainer.Set(cutsceneManager);
            cutsceneManager.RegisterTask(TaskOrder.Cutscene);
            
            // VfxManagerの生成
            var vfxManager = new VfxManager();
            ServiceContainer.Set(vfxManager);
            vfxManager.RegisterTask(TaskOrder.Effect);

            // BodyManagerの生成
            var bodyManager = new BodyManager(new BodyBuilder());
            ServiceContainer.Set(bodyManager);
            bodyManager.RegisterTask(TaskOrder.Body);
            
            // CollisionManagerの生成
            var collisionManager = new CollisionManager();
            ServiceContainer.Set(collisionManager);
            collisionManager.RegisterTask(TaskOrder.Collision);
            
            // ProjectileObjectManagerの生成
            var projectileObjectManager = new ProjectileObjectManager(collisionManager);
            ServiceContainer.Set(projectileObjectManager);
            projectileObjectManager.RegisterTask(TaskOrder.Projectile);
            
            // InputのTask登録
            Services.Get<BattleInput>().RegisterTask(TaskOrder.Input);

            // PlayerEntityの生成
            _playerActorEntity = new ActorEntity();
            yield return _playerActorEntity.SetupPlayerAsync(battleModel.PlayerModel, scope, ct)
                .ToCoroutine();
            
            // CameraTargetPoint制御用Logic追加
            var cameraTargetPointLogic = new CameraTargetPointLogic(_playerActorEntity, battleModel.AngleModel)
                .ScopeTo(scope);
            cameraTargetPointLogic.RegisterTask(TaskOrder.Logic);
            cameraTargetPointLogic.Activate();

            _layeredTime = new LayeredTime();
            _layeredTime.LocalTimeScale = 0.5f;
            _layeredTime.ScopeTo(scope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            if (Input.GetKeyDown(KeyCode.Space)) {
                MainSystem.Instance.Reboot(new BattleSceneSituation());
                return;
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                ParentContainer.Transition(new BattleSceneSituation());
                return;
            }

            // todo:コリジョンテスト
            if (Input.GetKeyDown(KeyCode.C)) {
                var collisionManager = Services.Get<CollisionManager>();
                var handle = collisionManager.Register(new SphereCollision(Vector3.forward * 5, 10), -1, null, result => {
                    Debug.Log($"Hit:{result.collider.name}");
                });
                
                Observable.TimerFrame(50)
                    .Subscribe(_ => handle.Dispose());
            }
            if (Input.GetKeyDown(KeyCode.V)) {
                var collisionManager = Services.Get<CollisionManager>();
                var handle = collisionManager.Register(new BoxCollision(Vector3.back * 5, Vector3.one * 10, Quaternion.identity), -1,null, result => {
                    Debug.Log($"Hit:{result.collider.name}");
                });
                
                Observable.TimerFrame(50)
                    .Subscribe(_ => handle.Dispose());
            }
            
            // カットシーンテスト
            if (Input.GetKeyDown(KeyCode.B)) {
                var cutsceneManager = Services.Get<CutsceneManager>();
                cutsceneManager.Play(_cutsceneScene, _layeredTime);
            }
            
            // BattleModel更新
            BattleModel.Get().Update();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="handle"></param>
        protected override void CleanupInternal(TransitionHandle handle) {
            // PlayerEntity削除
            _playerActorEntity.Dispose();

            // Inputのタスク登録解除
            var input = Services.Get<BattleInput>();
            input.UnregisterTask();

            base.CleanupInternal(handle);
        }

        /// <summary>
        /// PlayerMasterの読み込み
        /// </summary>
        private async UniTask<(BattlePlayerMasterData, PlayerActorSetupData, PlayerActorActionData[])> LoadPlayerMasterAsync(string playerId, IScope unloadScope, CancellationToken ct) {
            // マスター本体の読み込み
            var masterData = 
                await new BattlePlayerMasterDataAssetRequest(playerId)
                    .LoadAsync(unloadScope, ct);

            var setupData = default(PlayerActorSetupData);
            var actionDataList = new PlayerActorActionData[masterData.playerActorGeneralActionDataIds.Length];

            var tasks = new List<UniTask>();
            // Actor初期化用データ読み込み
            tasks.Add(new PlayerActorSetupDataAssetRequest(masterData.playerActorSetupDataId)
                .LoadAsync(unloadScope, ct)
                .ContinueWith(x => setupData = x));

            // ActionData読み込み
            tasks.AddRange(masterData.playerActorGeneralActionDataIds
                .Select((x, i) => new PlayerActorActionDataAssetRequest(x)
                    .LoadAsync(unloadScope, ct)
                    .ContinueWith(actionData => {
                        actionDataList[i] = actionData;
                    })
                ));

            await UniTask.WhenAll(tasks);
            return (masterData, setupData, actionDataList);
        }
    }
}