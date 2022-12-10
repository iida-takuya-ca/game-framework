using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 姿勢コンストレイント
    /// </summary>
    public class RotationConstraint : Constraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetAngles = Vector3.zero;
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
            _settings.offsetAngles = Vector3.zero;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            var space = _settings.space;

            // Rotation
            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(GetTargetRotation()) * transform.rotation;
            }
            else {
                offsetRotation = transform.rotation * Quaternion.Inverse(GetTargetRotation());
            }

            _settings.offsetAngles = offsetRotation.eulerAngles;
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            var space = _settings.space;
            var offset = Quaternion.Euler(_settings.offsetAngles);

            if (space == Space.Self) {
                transform.rotation = GetTargetRotation() * offset;
            }
            else {
                transform.rotation = offset * GetTargetRotation();
            }
        }
    }
}