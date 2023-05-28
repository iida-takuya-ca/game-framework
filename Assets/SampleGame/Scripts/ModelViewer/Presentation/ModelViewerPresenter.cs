using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.LogicSystems;
using UniRx;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// ModelViewer全体のPresenter
    /// </summary>
    public class ModelViewerPresenter : Logic {
        private ModelViewerModel _model;
        private EntityManager _entityManager;
        private EnvironmentManager _environmentManager;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerPresenter(ModelViewerModel model) {
            _model = model;
            _entityManager = Services.Get<EntityManager>();
            _environmentManager = Services.Get<EnvironmentManager>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var ct = scope.ToCancellationToken();
            
            // Actorの切り替え
            _model.PreviewActor.SetupData
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _entityManager.ChangePreviewActor(_model.PreviewActor);
                });
            
            // Environmentの切り替え
            _model.Environment.AssetId
                .TakeUntil(scope)
                .Subscribe(id => {
                    _environmentManager.ChangeEnvironmentAsync(id, ct).Forget();
                });
        }
    }
}
