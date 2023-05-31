using GameFramework.Core;
using GameFramework.ActorSystems;
using UniRx;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// PreviewActor制御用のPresenter
    /// </summary>
    public class PreviewActorPresenter : ActorEntityLogic {
        private PreviewActorModel _model;
        private PreviewActor _actor;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorPresenter(PreviewActorModel model, PreviewActor actor) {
            _model = model;
            _actor = actor;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // モーションの切り替え
            _model.CurrentAnimationClip
                .TakeUntil(scope)
                .Subscribe(clip => {
                    _actor.ChangeMotion(clip);
                });
            _model.CurrentAdditiveAnimationClip
                .TakeUntil(scope)
                .Subscribe(clip => {
                    _actor.ChangeAdditiveMotion(clip);
                });
        }
    }
}
