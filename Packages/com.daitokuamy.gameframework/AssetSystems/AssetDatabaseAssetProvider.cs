using System;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.AssetSystems {
    /// <summary>
    /// AssetDatabaseを使ったアセット提供用クラス
    /// </summary>
    public sealed class AssetDatabaseAssetProvider : IAssetProvider {
        /// <summary>
        /// アセット情報
        /// </summary>
        private class AssetInfo<T> : IAssetInfo<T>
            where T : Object {
            private T _asset;

            bool IAssetInfo<T>.IsDone => true;
            T IAssetInfo<T>.Asset => _asset;
            Exception IAssetInfo<T>.Exception => null;

            public AssetInfo(T asset) {
                _asset = asset;
            }

            public void Dispose() {
                // Unloadはしない
            }
        }

        /// <summary>
        /// シーンアセット情報
        /// </summary>
        private class SceneAssetInfo : ISceneAssetInfo {
            private SceneHolder _sceneHolder;
            
            bool ISceneAssetInfo.IsDone => true;
            SceneHolder ISceneAssetInfo.SceneHolder => _sceneHolder;
            Exception ISceneAssetInfo.Exception => new Exception("Not supported scene asset.");

            public SceneAssetInfo() {
                _sceneHolder = new SceneHolder();
            }
            
            public void Dispose() {
                // Unloadはしない
            }
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        AssetHandle<T> IAssetProvider.LoadAsync<T>(string address) {
#if UNITY_EDITOR
            // Address > Path変換
            var path = GetAssetPath<T>(address);
            
            // 読み込み処理
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            var info = new AssetInfo<T>(asset);
            return new AssetHandle<T>(info);
#else
            return AssetHandle<T>.Empty;
#endif
        }

        /// <summary>
        /// アセットが含まれているか
        /// </summary>
        bool IAssetProvider.Contains<T>(string address) {
#if UNITY_EDITOR
            // GUIDが存在しなければない扱い
            var guid = AssetDatabase.AssetPathToGUID(address);
            return !string.IsNullOrEmpty(guid);
#else
            return false;
#endif
        }

        /// <summary>
        /// シーンアセットの読み込み
        /// </summary>
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address, LoadSceneMode mode) {
#if UNITY_EDITOR
            var info = new SceneAssetInfo();
            return new SceneAssetHandle(info);
#else
            return SceneAssetHandle.Empty;
#endif
        }

        /// <summary>
        /// シーンアセットが含まれているか
        /// </summary>
        bool IAssetProvider.ContainsScene(string address) {
            // 常に失敗
            return false;
        }

        /// <summary>
        /// AddressをPathに変換
        /// </summary>
        private string GetAssetPath<T>(string address) {
#if USE_ADDRESSABLES && UNITY_EDITOR
            foreach (var locator in Addressables.ResourceLocators) {
                if (locator.Locate(address, typeof(T), out var list)) {
                    return list[0].InternalId;
                }
            }
#endif
            return address;
        }
    }
}