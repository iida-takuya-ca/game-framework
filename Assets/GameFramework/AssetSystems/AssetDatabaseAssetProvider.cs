using Object = UnityEngine.Object;
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
            string IAssetInfo<T>.Error => "";

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
#if UNITY_EDITOR
            // 読み込み処理
            var asset = AssetDatabase.LoadAssetAtPath<T>(address);
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
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address) {
#if UNITY_EDITOR
            var info = new SceneAssetInfo(address);
            return new SceneAssetHandle(info);
#else
            return SceneAssetHandle.Empty;
#endif
        }

        /// <summary>
        /// シーンアセットが含まれているか
        /// </summary>
        bool IAssetProvider.ContainsScene(string address) {
#if UNITY_EDITOR
            // GUIDが存在しなければない扱い
            var guid = AssetDatabase.AssetPathToGUID(address);
            return !string.IsNullOrEmpty(guid);
#else
            return false;
#endif
        }
    }
}