using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// プリロード用アセットストレージ
    /// </summary>
    public abstract class PreloadAssetStorage<TStorage, TAsset> : AssetStorage<TStorage>
        where TStorage : PreloadAssetStorage<TStorage, TAsset>, new()
        where TAsset : Object {
        // 読み込み待ち情報
        public struct Handle : IProcess {
            // 読み込み中のAssetHandleリスト
            private AssetHandle<TAsset>[] _assetHandles;
            
            // IEnumerator用
            object IEnumerator.Current => null;
            // 読み込み完了しているか
            public bool IsDone {
                get {
                    if (_assetHandles == null) {
                        return true;
                    }
                    
                    foreach (var handle in _assetHandles) {
                        if (!handle.IsDone) {
                            return false;
                        }
                    }

                    return true;
                }
            }
            // エラー
            public Exception Exception {
                get {
                    if (_assetHandles == null) {
                        return null;
                    }
                    
                    foreach (var handle in _assetHandles) {
                        if (handle.Exception != null) {
                            return handle.Exception;
                        }
                    }

                    return null;
                }
            }
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Handle(AssetHandle<TAsset>[] assetHandles) {
                _assetHandles = assetHandles;
            }

            /// <inheritdoc/>
            bool IEnumerator.MoveNext() {
                return !IsDone;
            }

            /// <inheritdoc/>
            void IEnumerator.Reset() {
                throw new NotImplementedException();
            }
        }
        
        // キャッシュ情報
        private class CacheInfo {
            public AssetHandle<TAsset> handle;
        }
        
        // キャッシュ
        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();

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
        protected Handle LoadAssetAsync(IEnumerable<AssetRequest<TAsset>> requests) {
            var handles = new List<AssetHandle<TAsset>>();
            
            foreach (var request in requests) {
                var address = request.Address;

                if (_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                    handles.Add(cacheInfo.handle);
                }
                else {
                    // ハンドルを取得してキャッシュ
                    var handle = request.LoadAsync(AssetManager);
                    _cacheInfos[address] = new CacheInfo {
                        handle = handle
                    };
                    handles.Add(handle);
                }
            }
            
            return new Handle(handles.ToArray());
        }
        
        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected Handle LoadAssetAsync(AssetRequest<TAsset> request) {
            var handles = new AssetHandle<TAsset>[1];
            var address = request.Address;
            
            if (_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                handles[0] = cacheInfo.handle;
            }
            else {
                // ハンドルを取得してキャッシュ
                var handle = request.LoadAsync(AssetManager);
                _cacheInfos[address] = new CacheInfo {
                    handle = handle
                };
                handles[0] = handle;
            }
            
            return new Handle(handles);
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
            foreach (var info in _cacheInfos.Values) {
                info.handle.Release();
            }
            _cacheInfos.Clear();
        }
    }
}