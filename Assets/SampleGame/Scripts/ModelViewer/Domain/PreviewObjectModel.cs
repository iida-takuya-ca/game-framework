using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 表示用オブジェクトモデル
    /// </summary>
    public class PreviewObjectModel : AutoIdModel<PreviewObjectModel> {
        private ReactiveProperty<ModelViewerBodyData> _bodyDataProperty = new();

        // Bodyに対する設定データ
        public IReadOnlyReactiveProperty<ModelViewerBodyData> BodyDataProperty => _bodyDataProperty;
        
        /// <summary>
        /// BodyDataの切り替え
        /// </summary>
        public void SetBodyData(ModelViewerBodyData bodyData) {
            // BodyData更新
            _bodyDataProperty.Value = bodyData;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private PreviewObjectModel(int id) 
            : base(id) {}
    }
}
