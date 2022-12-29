using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Attachmentの基底
    /// </summary>
    [ExecuteAlways]
    public abstract class Attachment : MonoBehaviour, IAttachment {
        // 更新モード
        public enum Mode {
            Update,
            LateUpdate,
            Manual,
        }

        [SerializeField, Tooltip("有効化")]
        private bool _active = false;
        [SerializeField, Tooltip("更新モード")]
        private Mode _updateMode = Mode.LateUpdate;
        [SerializeField, Tooltip("ターゲットリスト")]
        private ConstraintResolver.TargetSource[] _sources = Array.Empty<ConstraintResolver.TargetSource>();

        // 初期化済みか
        private bool _initialized;

        // 更新モード
        public Mode UpdateMode {
            get => _updateMode;
            set => _updateMode = value;
        }
        // ターゲットリスト
        public ConstraintResolver.TargetSource[] Sources {
            set {
                if (!_initialized) {
                    Initialize();
                    _initialized = true;
                }
                _sources = value;
                if (Resolver != null) {
                    Resolver.Sources = _sources;
                }
            }
        }
        // Transform制御用インスタンス
        protected abstract ConstraintResolver Resolver { get; }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void ManualUpdate() {
            if (_updateMode != Mode.Manual) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public void TransferOffset() {
            Resolver.TransferOffset();
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public void ResetOffset() {
            Resolver.ResetOffset();
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public void ApplyTransform() {
            Resolver.ApplyTransform();
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// 更新処理(内部用)
        /// </summary>
        private void UpdateInternal() {
            if (_sources.Length <= 0) {
                return;
            }

#if UNITY_EDITOR
            if (!_active && !Application.isPlaying) {
                return;
            }
#endif
            if (!_initialized) {
                Initialize();
                _initialized = true;
            }

            ApplyTransform();
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            Sources = _sources;
            if (!_initialized) {
                Initialize();
                _initialized = true;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (_updateMode != Mode.Update && _updateMode != Mode.Manual && Application.isPlaying) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            if (_updateMode != Mode.LateUpdate) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// 値変更時の処理
        /// </summary>
        private void OnValidate() {
            Sources = _sources;
        }
    }
}