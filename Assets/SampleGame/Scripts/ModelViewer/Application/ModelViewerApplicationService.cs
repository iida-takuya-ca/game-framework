using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューア用のアプリケーション層サービス
    /// </summary>
    public class ModelViewerApplicationService {
        private ModelViewerModel _model;
        private ModelViewerRepository _repository;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerApplicationService(ModelViewerModel model, ModelViewerRepository repository) {
            _model = model;
            _repository = repository;
        }

        /// <summary>
        /// 表示モデルの変更
        /// </summary>
        public async UniTask<PreviewActorSetupData> ChangePreviewActorAsync(string setupDataId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            // 設定ファイルを読み込み
            var result = await _repository.LoadActorDataAsync(setupDataId, ct);
            
            // Modelに反映
            _model.PreviewActor.Setup(setupDataId, result);

            return result;
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeEnvironment(string assetId) {
            // Modelに反映
            _model.Environment.SetAssetId(assetId);
        }
    }
}
