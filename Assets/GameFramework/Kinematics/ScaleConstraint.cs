using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 拡縮コンストレイント
    /// </summary>
    public class ScaleConstraint : Constraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Vector3 offsetScale = Vector3.one;
        }

        [SerializeField, Tooltip("コンストレイント設定")]
        private ConstraintSettings _settings = null;

        // コンストレイント設定
        public ConstraintSettings Settings {
            get => _settings;
            set => _settings = value;
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            _settings.offsetScale = Vector3.one;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            // Scale
            var targetScale = GetTargetLocalScale();
            var localScale = transform.localScale;
            _settings.offsetScale = new Vector3
            (
                localScale.x / targetScale.x,
                localScale.y / targetScale.y,
                localScale.z / targetScale.z
            );
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            transform.localScale = Vector3.Scale(GetTargetLocalScale(), _settings.offsetScale);
        }
    }
}