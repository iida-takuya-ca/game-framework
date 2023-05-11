using System;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセット格納クラス
    /// </summary>
    public interface IAssetStorage : IDisposable {
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize(AssetManager assetManager);
    }
}