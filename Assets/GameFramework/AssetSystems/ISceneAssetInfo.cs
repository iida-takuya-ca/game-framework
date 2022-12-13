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
        /// 読み込み済みシーンパス
        /// </summary>
        string ScenePath { get; }
        
        /// <summary>
        /// エラーメッセージ
        /// </summary>
        string Error { get; }
    }
}