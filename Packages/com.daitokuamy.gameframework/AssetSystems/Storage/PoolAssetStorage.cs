using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// プール管理用アセットストレージ
    /// </summary>
    public abstract class PoolAssetStorage<TStorage, TAsset> : AssetStorage<TStorage>
        where TStorage : PoolAssetStorage<TStorage, TAsset>, new()
        where TAsset : Object {
        // キャッシュ情報
        private class CacheInfo {
            public AssetHandle<TAsset> handle;
        }

        // キャッシュ
        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();
        // 読み込み順番管理
        private readonly List<string> _fetchAddresses = new();

        // 同時キャッシュ数
        private int _amount = 3;
        public int Amount {
            get => _amount;
            set {
                _amount = Mathf.Max(value, 0);
                FetchAddress();
            }
        }

        /// <summary>
        /// 読み込んだアセットの全解放
        /// </summary>
        public static void Clear() {
            Get().UnloadAssets();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            UnloadAssets();
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected AssetHandle<TAsset> LoadAssetAsync(AssetRequest<TAsset> request) {
            var address = request.Address;

            // Addressのフェッチ
            FetchAddress(address);

            // 既にキャッシュがある場合、キャッシュ経由で読み込みを待つ
            if (_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                return cacheInfo.handle;
            }

            // キャッシュがない場合、LoadingStatusObserverを使って読み込み
            cacheInfo = new CacheInfo();
            cacheInfo.handle = request.LoadAsync(AssetManager);
            _cacheInfos[address] = cacheInfo;
            return cacheInfo.handle;
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        protected TAsset GetAsset(string address) {
            if (!_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                return null;
            }

            return cacheInfo.handle.Asset;
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        protected TAsset GetAsset(AssetRequest<TAsset> request) {
            return GetAsset(request.Address);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        protected void UnloadAssets() {
            var keys = _cacheInfos.Keys.ToArray();

            foreach (var key in keys) {
                RemoveCache(key);
            }
        }

        /// <summary>
        /// アドレスのフェッチ（最大数を超えたアセットは自動でアンロード）
        /// </summary>
        private void FetchAddress(string address = null) {
            if (!string.IsNullOrEmpty(address)) {
                _fetchAddresses.Remove(address);
                _fetchAddresses.Add(address);
            }

            while (_fetchAddresses.Count > Amount) {
                // 古い物は削除
                RemoveCache(_fetchAddresses[0]);
            }
        }

        /// <summary>
        /// キャッシュの解放
        /// </summary>
        private void RemoveCache(string address) {
            if (!_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                Debug.unityLogger.LogError(GetType().Name, $"Not found cache info. {address}");
                return;
            }

            if (cacheInfo.handle.IsValid) {
                cacheInfo.handle.Release();
            }

            _cacheInfos.Remove(address);
            _fetchAddresses.Remove(address);
        }
    }
}