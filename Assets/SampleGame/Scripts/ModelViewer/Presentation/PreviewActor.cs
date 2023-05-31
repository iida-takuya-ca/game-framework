using System;
using System.Linq;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// オブジェクトプレビュー用のアクター
    /// </summary>
    public class PreviewActor : Actor {
        private MotionController _motionController;
        private GimmickController _gimmickController;
        private AvatarController _avatarController;
        
        public PreviewActorSetupData SetupData { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActor(Body body, PreviewActorSetupData setupData)
            : base(body) {
            SetupData = setupData;
            _motionController = body.GetController<MotionController>();
            _gimmickController = body.GetController<GimmickController>();
            _avatarController = body.GetController<AvatarController>();
        }

        /// <summary>
        /// モーションの変更
        /// </summary>
        public void ChangeMotion(AnimationClip clip) {
            if (_motionController == null) {
                return;
            }
            
            _motionController.Player.Change(0, clip, 0.0f);
        }

        /// <summary>
        /// 加算モーションの変更
        /// </summary>
        public void ChangeAdditiveMotion(AnimationClip clip) {
            if (_motionController == null) {
                return;
            }
            
            _motionController.Player.Change(1, clip, 0.0f);
        }

        /// <summary>
        /// ギミックキーの一覧を取得
        /// </summary>
        /// <returns></returns>
        public string[] GetGimmickKeys() {
            if (_gimmickController == null) {
                return Array.Empty<string>();
            }
            
            return _gimmickController.GetKeys();
        }

        /// <summary>
        /// メッシュアバターの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            if (_avatarController == null) {
                return;
            }

            var info = SetupData.meshAvatarInfos.FirstOrDefault(x => x.key == key);
            if (info == null) {
                return;
            }

            if (index < 0 || index >= info.prefabs.Length) {
                _avatarController.ResetPart(key);
            }
            else {
                _avatarController.ChangePart(new MeshAvatarResolver(key, info.prefabs[index]));
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // RootMotionを無効化
            var animator = Body.GetComponent<Animator>();
            if (animator != null) {
                animator.applyRootMotion = false;
            }
            
            // 加算レイヤーの追加
            if (_motionController != null) {
                _motionController.Player.BuildAdditionalLayers(
                    new MotionPlayer.LayerSettings {
                        additive = true,
                        avatarMask = null
                    });
            }
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            // 加算レイヤーの削除
            if (_motionController != null) {
                _motionController.Player.ResetAdditionalLayers();
            }
            
            base.DeactivateInternal();
        }
    }
}
