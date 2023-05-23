using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 環境情報用モデル
    /// </summary>
    public class EnvironmentModel : AutoIdModel<EnvironmentModel> {
        private ReactiveProperty<string> _assetIdProperty = new();

        // 環境情報用のアセットID
        public IReadOnlyReactiveProperty<string> AssetIdProperty => _assetIdProperty;
        
        /// <summary>
        /// BodyDataの切り替え
        /// </summary>
        public void SetAssetId(string assetId) {
            _assetIdProperty.Value = assetId;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private EnvironmentModel(int id) 
            : base(id) {}
    }
}
