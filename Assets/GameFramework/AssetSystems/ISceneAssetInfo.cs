using System;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーンアセット情報インターフェース
    /// </summary>
    public interface ISceneAssetInfo : IDisposable {
        /// <summary>
        /// 読み込み完了しているか
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// シーンの読み込み情報
        /// </summary>
        SceneHolder SceneHolder { get; }
        
        /// <summary>
        /// エラー
        /// </summary>
        Exception Exception { get; }
    }
}