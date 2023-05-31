using GameFramework.Core;
using GameFramework.ModelSystems;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューア用のルートモデル
    /// </summary>
    public class ModelViewerModel : SingleModel<ModelViewerModel> {
        // 基本データ
        public ModelViewerSetupData SetupData { get; private set; }
        // 表示用オブジェクトのモデル
        public PreviewActorModel PreviewActorModel { get; private set; }
        // 環境用のモデル
        public EnvironmentModel EnvironmentModel { get; private set; }
        // 設定用のモデル
        public SettingsModel SettingsModel { get; private set; }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(ModelViewerSetupData setupData) {
            SetupData = setupData;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            PreviewActorModel = PreviewActorModel.Create()
                .ScopeTo(scope);
            EnvironmentModel = EnvironmentModel.Create()
                .ScopeTo(scope);
            SettingsModel = SettingsModel.Create()
                .ScopeTo(scope);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ModelViewerModel(object empty) 
            : base(empty) {}
    }
}
