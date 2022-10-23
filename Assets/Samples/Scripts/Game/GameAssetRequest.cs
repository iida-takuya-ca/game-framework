using System;
using GameFramework.Core;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SampleGame {
    /// <summary>
    /// Sample用のAssetRequest基底
    /// </summary>
    public abstract class GameAssetRequest<T> : AssetRequest
        where T : Object {
        public GameAssetRequest(string path)
            : base(path) {}

        /// <summary>
        /// アセットの読み込み処理
        /// </summary>
        public IObservable<T> LoadAsync() {
            return Resources.LoadAsync<T>(Path)
                .AsAsyncOperationObservable()
                .Select(op => (T)op.asset);
        }
    }

    /// <summary>
    /// BodyPrefabのAssetRequest基底
    /// </summary>
    public abstract class BodyPrefabAssetRequest : GameAssetRequest<GameObject> {
        public BodyPrefabAssetRequest(string relativePath)
            : base($"Assets/Samples/BodyAssets/{relativePath}"){}
    }

    /// <summary>
    /// PlayerPrefabのAssetRequest
    /// </summary>
    public class PlayerPrefabAssetRequest : BodyPrefabAssetRequest {
        public PlayerPrefabAssetRequest(string assetKey)
            : base($"Player/{assetKey}/prfb_{assetKey}.prefab"){}
    }
}
