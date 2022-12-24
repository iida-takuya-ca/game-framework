using System;
using UnityEngine.ResourceManagement.ResourceProviders;

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
        SceneInstance SceneInstance { get; }
        
        /// <summary>
        /// エラー
        /// </summary>
        Exception Exception { get; }
    }
}