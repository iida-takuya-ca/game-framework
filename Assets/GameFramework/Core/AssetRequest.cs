namespace GameFramework.Core {
    /// <summary>
    /// アセット読み込みリクエスト用インターフェース
    /// </summary>
    public interface IAssetRequest {
        // 読み込みに使うPath
        string Path { get; }
    }
    
    /// <summary>
    /// アセット読み込みリクエスト
    /// </summary>
    public class AssetRequest : IAssetRequest {
        // 読み込みに使うPath
        private readonly string _path;
        public string Path => _path;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AssetRequest(string path) {
            _path = path;
        }
    }
}