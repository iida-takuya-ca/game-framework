using System;
using System.Collections.Generic;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SampleGame {
    /// <summary>
    /// Sample用のAssetRequest基底
    /// </summary>
    public abstract class AssetRequest<T> : GameFramework.AssetSystems.AssetRequest<T>
        where T : Object {
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase };

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="unloadScope">解放スコープ</param>
        public IObservable<T> LoadAsync(IScope unloadScope) {
            return Observable.Create<T>(observer => {
                var handle = LoadAsync(Services.Get<AssetManager>(), unloadScope);
                if (!handle.IsValid) {
                    observer.OnError(new KeyNotFoundException($"Load failed. {Address}"));
                    return Disposable.Empty;
                }

                if (handle.IsDone) {
                    observer.OnNext(handle.Asset);
                    observer.OnCompleted();
                    return Disposable.Empty;
                }
                
                // 読み込みを待つ
                return Observable.EveryUpdate()
                    .Subscribe(_ => {
                        if (handle.Exception != null) {
                            observer.OnError(handle.Exception);
                        }
                        else if (handle.IsDone) {
                            observer.OnNext(handle.Asset);
                            observer.OnCompleted();
                        }
                    });
            });
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected string GetPath(string relativePath) {
            return $"Assets/SampleGame/{relativePath}";
        }
    }
    
    /// <summary>
    /// Sample用のSceneAssetRequest基底
    /// </summary>
    public class SceneAssetRequest : GameFramework.AssetSystems.SceneAssetRequest {
        private LoadSceneMode _mode;
        private string _address;

        public override LoadSceneMode Mode => _mode;
        public override string Address => _address;
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="relativePath">Scenes以下の相対パス</param>
        /// <param name="mode">Sceneの読み込みモード</param>
        public SceneAssetRequest(string relativePath, LoadSceneMode mode) {
            _address = $"Assets/SampleGame/Scenes/{relativePath}";
            _mode = mode;
        }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="unloadScope">解放スコープ</param>
        public IObservable<SceneInstance> LoadAsync(IScope unloadScope) {
            return Observable.Create<SceneInstance>(observer => {
                var handle = LoadAsync(Services.Get<AssetManager>(), unloadScope);
                if (!handle.IsValid) {
                    observer.OnError(new KeyNotFoundException($"Load scene failed. {Address}"));
                    return Disposable.Empty;
                }

                if (handle.IsDone) {
                    observer.OnNext(handle.SceneInstance);
                    observer.OnCompleted();
                    return Disposable.Empty;
                }
                
                // 読み込みを待つ
                return Observable.EveryUpdate()
                    .Subscribe(_ => {
                        if (handle.Exception != null) {
                            observer.OnError(handle.Exception);
                        }
                        else if (handle.IsDone) {
                            observer.OnNext(handle.SceneInstance);
                            observer.OnCompleted();
                        }
                    });
            });
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected string GetPath(string relativePath) {
            return $"Assets/SampleGame/{relativePath}";
        }
    }

    /// <summary>
    /// BodyPrefabのAssetRequest基底
    /// </summary>
    public abstract class BodyPrefabAssetRequest : AssetRequest<GameObject> {
        public override string Address { get; }

        public BodyPrefabAssetRequest(string relativePath) {
            Address = GetPath($"BodyAssets/{relativePath}");
        }
    }

    /// <summary>
    /// DataのAssetRequest基底
    /// </summary>
    public abstract class DataAssetRequest<T> : AssetRequest<T>
        where T : Object {
        public override string Address { get; }

        public DataAssetRequest(string relativePath) {
            Address = GetPath($"Data/{relativePath}");
        }
    }

    /// <summary>
    /// PlayerPrefabのAssetRequest
    /// </summary>
    public class PlayerPrefabAssetRequest : BodyPrefabAssetRequest {
        public PlayerPrefabAssetRequest(string assetKey)
            : base($"Player/{assetKey}/Models/prfb_{assetKey}.prefab") {
        }
    }

    /// <summary>
    /// PlayerActorSetupDataのAssetRequest
    /// </summary>
    public class PlayerActorSetupDataAssetRequest : DataAssetRequest<PlayerActorSetupData> {
        public PlayerActorSetupDataAssetRequest(string assetKey)
            : base($"PlayerActorSetup/dat_player_actor_setup_{assetKey}.asset") {
        }
    }

    /// <summary>
    /// PlayerActorActionDataのAssetRequest
    /// </summary>
    public class PlayerActorActionDataAssetRequest : DataAssetRequest<PlayerActorActionData> {
        public PlayerActorActionDataAssetRequest(string assetKey)
            : base($"PlayerActorAction/dat_player_actor_action_{assetKey}.asset") {
        }
    }

    /// <summary>
    /// BattlePlayerMasterDataのAssetRequest
    /// </summary>
    public class BattlePlayerMasterDataAssetRequest : DataAssetRequest<BattlePlayerMasterData> {
        public BattlePlayerMasterDataAssetRequest(string assetKey)
            : base($"Battle/BattlePlayerMaster/dat_battle_player_master_{assetKey}.asset") {
        }
    }

    /// <summary>
    /// ModelViewerBodyDataのAssetRequest
    /// </summary>
    public class ModelViewerBodyDataRequest : DataAssetRequest<ModelViewerBodyData> {
        public ModelViewerBodyDataRequest(string assetKey)
            : base($"ModelViewer/Body/dat_model_viewer_body_{assetKey}.asset") {
        }
    }
}