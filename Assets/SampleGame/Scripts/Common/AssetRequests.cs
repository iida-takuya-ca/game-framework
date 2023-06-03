using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;
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
        /// <param name="ct">読み込みキャンセル用</param>
        public async UniTask<T> LoadAsync(IScope unloadScope, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            var handle = LoadAsync(Services.Get<AssetManager>(), unloadScope);
            await handle.ToUniTask(cancellationToken:ct);
            if (!handle.IsValid) {
                Debug.LogException(new KeyNotFoundException($"Load failed. {Address}"));
                return null;
            }

            return handle.Asset;
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected virtual string GetPath(string relativePath) {
            return $"Assets/SampleGame/{relativePath}";
        }
    }
    
    /// <summary>
    /// Sample用のSceneAssetRequest基底
    /// </summary>
    public abstract class SceneAssetRequest : GameFramework.AssetSystems.SceneAssetRequest {
        private LoadSceneMode _mode;
        private string _address;

        public override LoadSceneMode Mode => _mode;
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">読み込みパス</param>
        /// <param name="mode">Sceneの読み込みモード</param>
        public SceneAssetRequest(LoadSceneMode mode) {
            _mode = mode;
        }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="activate">アクティブ化するか</param>
        /// <param name="unloadScope">解放スコープ</param>
        /// <param name="ct">Taskキャンセル用Token</param>
        public async UniTask<Scene> LoadAsync(bool activate, IScope unloadScope, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            var handle = LoadAsync(Services.Get<AssetManager>(), unloadScope);
            if (!handle.IsValid) {
                Debug.LogException(new KeyNotFoundException($"Load failed. {Address}"));
                return default;
            }

            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Exception != null) {
                Debug.LogException(handle.Exception);
                return default;
            }

            var holder = handle.Scene;
            if (activate) {
                await handle.ActivateAsync()
                    .ToUniTask(cancellationToken: ct);
            }

            return holder;
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected string GetProjectPath(string relativePath) {
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
    /// ActorAssetのAssetRequest基底
    /// </summary>
    public abstract class ActorAssetRequest<T> : AssetRequest<T>
        where T : Object {
        protected sealed override string GetPath(string relativePath) {
            return base.GetPath($"ActorAssets/{relativePath}");
        }
    }

    /// <summary>
    /// DataのAssetRequest基底
    /// </summary>
    public abstract class DataAssetRequest<T> : AssetRequest<T>
        where T : Object {
        protected sealed override string GetPath(string relativePath) {
            return base.GetPath($"DataAssets/{relativePath}");
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
    public class PlayerActorSetupDataAssetRequest : ActorAssetRequest<PlayerActorSetupData> {
        public override string Address { get; }
        
        public PlayerActorSetupDataAssetRequest(string assetKey) {
            var actorId = assetKey.Substring(0, "pl000".Length);
            Address = GetPath($"Player/{actorId}/dat_player_actor_setup_{assetKey}.asset");
        }
    }

    /// <summary>
    /// PlayerActorActionDataのAssetRequest
    /// </summary>
    public class PlayerActorActionDataAssetRequest : ActorAssetRequest<PlayerActorActionData> {
        public override string Address { get; }
        
        public PlayerActorActionDataAssetRequest(string assetKey) {
            var actorId = assetKey.Substring(0, "pl000".Length);
            Address = GetPath($"Player/{actorId}/Actions/dat_player_actor_action_{assetKey}.asset");
        }
    }

    /// <summary>
    /// フィールドシーン用のAssetRequest
    /// </summary>
    public class FieldSceneAssetRequest : SceneAssetRequest {
        // 読み込みAddress
        public override string Address { get; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">fld000</param>
        public FieldSceneAssetRequest(string assetKey) : base(LoadSceneMode.Additive) {
            Address = GetProjectPath($"EnvironmentAssets/Field/{assetKey}/{assetKey}.unity");
        }
    }
}