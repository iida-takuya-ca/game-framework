using System;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// モデルビューア管理用クラス
    /// </summary>
    public class ModelViewerManager : LateUpdatableTaskBehaviour {
        [SerializeField, Tooltip("ビューア用のデータ")]
        private ModelViewerData _data;

        // Body生成中のScope
        private DisposableScope _bodyScope;

        // 読み込み可能なBodyDataのIdリスト
        public string[] BodyDataIds => _data.bodyDataIds;
        // 現在使用中のBodyData
        public ModelViewerBodyData CurrentBodyData { get; private set; }
        // 現在使用中のBody
        public Body CurrentBody { get; private set; }

        /// <summary>
        /// Bodyの生成
        /// </summary>
        public void SetupBody(string bodyDataId, Action onCreated) {
            CleanupBody();

            _bodyScope = new DisposableScope();

            var bodyManager = Services.Get<BodyManager>();
            
            new ModelViewerBodyDataRequest(bodyDataId)
                .LoadAsync(_bodyScope)
                .Subscribe(data => {
                    CurrentBodyData = data;
                    // Bodyの生成
                    CurrentBody = bodyManager.CreateFromPrefab(data.prefab);
                    onCreated?.Invoke();
                })
                .ScopeTo(_bodyScope);
        }

        /// <summary>
        /// 現在生成されているBodyの削除
        /// </summary>
        public void CleanupBody() {
            if (CurrentBodyData == null) {
                return;
            }

            if (CurrentBody != null) {
                CurrentBody.Dispose();
                CurrentBody = null;
            }

            if (_bodyScope != null) {
                _bodyScope.Dispose();
                _bodyScope = null;
            }

            CurrentBodyData = null;
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(int index) {
            if (CurrentBody == null) {
                return;
            }

            if (index < 0 || index >= CurrentBodyData.animationClips.Length) {
                return;
            }

            var clip = CurrentBodyData.animationClips[index];
            if (clip == null) {
                return;
            }

            var motionController = CurrentBody.GetController<MotionController>();
            if (motionController == null) {
                return;
            }

            motionController.Player.SetMotion(clip, 0.2f);
        }
    }
}
