using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// エイムコンストレイント
    /// </summary>
    public class AimConstraint : Constraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetAngles = Vector3.zero;
        }

        [SerializeField, Tooltip("コンストレイント設定")]
        private ConstraintSettings _settings = null;
        [SerializeField, Tooltip("正面のベクトル")]
        private Vector3 _forwardVector = Vector3.forward;
        [SerializeField, Tooltip("上のベクトル")]
        private Vector3 _upVector = Vector3.up;
        [SerializeField, Tooltip("UpベクトルをさすTransform(未指定はデフォルト)")]
        private Transform _worldUpObject = null;

        // コンストレイント設定
        public ConstraintSettings Settings {
            get => _settings;
            set => _settings = value;
        }
        // 正面のベクトル
        public Vector3 ForwardVector {
            set => _forwardVector = value;
        }
        // 上のベクトル
        public Vector3 UpVector {
            set => _upVector = value;
        }
        // UpベクトルをさすTransform
        public Transform WorldUpObject {
            set => _worldUpObject = value;
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
            var axisRotation = Quaternion.Inverse(Quaternion.LookRotation(_forwardVector, _upVector));
            var upVector = _worldUpObject != null ? _worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - transform.position, upVector) * axisRotation;

            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(baseRotation) * transform.rotation;
            }
            else {
                offsetRotation = transform.rotation * Quaternion.Inverse(baseRotation);
            }

            _settings.offsetAngles = offsetRotation.eulerAngles;
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            var space = _settings.space;
            var offsetRotation = Quaternion.Euler(_settings.offsetAngles);
            var axisRotation = Quaternion.Inverse(Quaternion.LookRotation(_forwardVector, _upVector));
            var upVector = _worldUpObject != null ? _worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - transform.position, upVector) * axisRotation;

            if (space == Space.Self) {
                transform.rotation = baseRotation * offsetRotation;
            }
            else {
                transform.rotation = offsetRotation * baseRotation;
            }
        }
    }
}