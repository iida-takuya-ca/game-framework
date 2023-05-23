using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.PlayableSystems;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// オブジェクトプレビュー用のアクター
    /// </summary>
    public class PreviewObjectActor : Actor {
        private MotionController _motionController;
        private GimmickController _gimmickController;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewObjectActor(Body body)
            : base(body) {
            _motionController = body.GetController<MotionController>();
        }

        /// <summary>
        /// モーションの変更
        /// </summary>
        public void ChangeMotion(AnimationClip clip) {
            _motionController.Player.Change(0, clip, 0.0f);
        }

        /// <summary>
        /// 加算モーションの変更
        /// </summary>
        public void ChangeAdditiveMotion(AnimationClip clip) {
            _motionController.Player.Change(1, clip, 0.0f);
        }

        public string[] GetGimmickKeys() {
            return _gimmickController.GetKeys();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // 加算レイヤーの追加
            _motionController.Player.BuildAdditionalLayers(
                new MotionPlayer.LayerSettings {
                    additive = true,
                    avatarMask = null
                });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            // 加算レイヤーの削除
            _motionController.Player.ResetAdditionalLayers();
            
            base.DeactivateInternal();
        }
    }
}
