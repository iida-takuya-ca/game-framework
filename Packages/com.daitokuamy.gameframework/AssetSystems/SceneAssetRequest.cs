using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーンアセット読み込みリクエスト
    /// </summary>
    public abstract class SceneAssetRequest {
        // 読み込みモード
        public abstract LoadSceneMode Mode { get; }
        // 読み込み用のAddress
        public abstract string Address { get; }
        // 読み込みに使用するProviderのIndex配列（順番にフォールバック）
        public abstract int[] ProviderIndices { get; }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        /// <param name="unloadScope">解放スコープ</param>
        public SceneAssetHandle LoadAsync(AssetManager assetManager, IScope unloadScope) {
            var address = Address;
            var handle = SceneAssetHandle.Empty;

            // 読み込みに使用できるProviderを探し、それを使って読み込みを開始する
            for (var i = 0; i < ProviderIndices.Length; i++) {
                var provider = assetManager.GetProvider(ProviderIndices[i]);
                if (provider == null) {
                    continue;
                }

                if (!provider.ContainsScene(address)) {
                    continue;
                }

                handle = provider.LoadSceneAsync(address, Mode);
                break;
            }

            if (handle.IsValid) {
                // 解放処理を仕込む
                unloadScope.OnExpired += () => handle.Release();
            }
            else {
                Debug.LogError($"Not found provider. [{address}]");
            }

            return handle;
        }
    }
}