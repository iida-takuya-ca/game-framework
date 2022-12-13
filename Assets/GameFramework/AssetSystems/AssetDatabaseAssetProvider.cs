using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
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
            private AsyncOperation _asyncOperation;
            private string _path;
            private Scene? _scene;
            
            bool ISceneAssetInfo.IsDone => _asyncOperation == null || _asyncOperation.isDone;
            Scene ISceneAssetInfo.Scene {
                get {
                    if (_scene.HasValue) {
                        return _scene.Value;
                    }

                    _scene = SceneManager.GetSceneByPath(_path);
                    return _scene.Value;
                }
            }

            public SceneAssetInfo(AsyncOperation asyncOperation, string path) {
                _asyncOperation = asyncOperation;
                _path = path;
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
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address, LoadSceneMode sceneMode) {
#if UNITY_EDITOR
            var asyncOp = EditorSceneManager.LoadSceneAsyncInPlayMode(address, new LoadSceneParameters {
                loadSceneMode = sceneMode,
                localPhysicsMode = LocalPhysicsMode.None
            });
            var info = new SceneAssetInfo(asyncOp, address);
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