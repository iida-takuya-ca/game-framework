using GameFramework.Core;
using GameFramework.ModelSystems;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューア用のルートモデル
    /// </summary>
    public class ModelViewerModel : SingleModel<ModelViewerModel> {
        // 基本データ
        public ModelViewerData Data { get; private set; }
        // 表示用オブジェクトのモデル
        public PreviewActorModel PreviewActor { get; private set; }
        // 環境用のモデル
        public EnvironmentModel Environment { get; private set; }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(ModelViewerData data) {
            Data = data;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            PreviewActor = PreviewActorModel.Create()
                .ScopeTo(scope);
            Environment = EnvironmentModel.Create()
                .ScopeTo(scope);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ModelViewerModel(object empty) 
            : base(empty) {}
    }
}
