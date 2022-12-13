using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// Resourcesを使ったアセット提供用クラス
    /// </summary>
    public sealed class ResourcesAssetProvider : IAssetProvider {
        /// <summary>
        /// アセット情報
        /// </summary>
        private class AssetInfo<T> : IAssetInfo<T>
            where T : Object {
            private ResourceRequest _request;

            bool IAssetInfo<T>.IsDone => _request == null || _request.isDone;
            T IAssetInfo<T>.Asset => (T)_request?.asset;
            string IAssetInfo<T>.Error => "";

            public AssetInfo(ResourceRequest request) {
                _request = request;
            }

            public void Dispose() {
                // Unloadはしない
            }
        }

        /// <summary>
        /// シーンアセット情報
        /// </summary>
        private class SceneAssetInfo : ISceneAssetInfo {
            private string _scenePath;
            
            bool ISceneAssetInfo.IsDone => true;
            string ISceneAssetInfo.ScenePath => _scenePath;
            string ISceneAssetInfo.Error => "";

            public SceneAssetInfo(string scenePath) {
                _scenePath = scenePath;
            }
            
            public void Dispose() {
                // Unloadはしない
            }
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        AssetHandle<T> IAssetProvider.LoadAsync<T>(string address) {
            var resourcesPath = GetResourcesPath(address);
            // 読み込み開始
            var request = Resources.LoadAsync<T>(resourcesPath);
            var info = new AssetInfo<T>(request);
            return new AssetHandle<T>(info);
        }

        /// <summary>
        /// アセットが含まれているか
        /// </summary>
        bool IAssetProvider.Contains<T>(string address) {
            // Resourcesフォルダ以下にあれば含まれている扱いにする
            return address.Contains("/Resources/");
        }

        /// <summary>
        /// シーンアセットの読み込み
        /// </summary>
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address) {
            var info = new SceneAssetInfo(address);
            return new SceneAssetHandle(info);
        }

        /// <summary>
        /// シーンアセットが含まれているか
        /// </summary>
        bool IAssetProvider.ContainsScene(string address) {
            // 常に失敗
            return false;
        }

        /// <summary>
        /// Resourcesから読み込む際のPathに変換
        /// </summary>
        private string GetResourcesPath(string address) {
            // Resources以下のパスに変換
            var index = address.LastIndexOf("/Resources/", StringComparison.Ordinal);
            if (index >= 0) {
                address = address.Substring(index + "/Resources/".Length);
            }
            
            // 拡張子を削除
            var extension = Path.GetExtension(address);
            if (!string.IsNullOrEmpty(extension)) {
                address = address.Replace(extension, "");
            }

            return address;
        }
    }
}