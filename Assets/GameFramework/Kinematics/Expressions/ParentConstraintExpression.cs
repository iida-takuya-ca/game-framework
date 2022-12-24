using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 追従コンストレイント
    /// </summary>
    public class ParentConstraintExpression : ConstraintExpression, IJobParentConstraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition = Vector3.zero;
            public Vector3 offsetAngles = Vector3.zero;
            public Vector3 offsetScale = Vector3.one;
        }

        [SerializeField, Tooltip("コンストレイント設定")]
        private ConstraintSettings _settings = null;

        // コンストレイント設定
        public ConstraintSettings Settings {
            set { _settings = value; }
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            _settings.offsetPosition = Vector3.zero;
            _settings.offsetAngles = Vector3.zero;
            _settings.offsetScale = Vector3.one;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            var space = _settings.space;
            // Position
            var offsetPosition = transform.position - GetTargetPosition();

            if (space == Space.Self) {
                offsetPosition = transform.InverseTransformVector(offsetPosition);
            }

            _settings.offsetPosition = offsetPosition;

            // Rotation
            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(GetTargetRotation()) * transform.rotation;
            }
            else {
                offsetRotation = transform.rotation * Quaternion.Inverse(GetTargetRotation());
            }

            _settings.offsetAngles = offsetRotation.eulerAngles;

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
            var space = _settings.space;
            var offsetPosition = _settings.offsetPosition;
            var offsetRotation = Quaternion.Euler(_settings.offsetAngles);
            var offsetScale = _settings.offsetScale;

            if (space == Space.Self) {
                offsetPosition = transform.TransformVector(offsetPosition);
                transform.rotation = GetTargetRotation() * offsetRotation;
            }
            else {
                transform.rotation = offsetRotation * GetTargetRotation();
            }

            transform.position = GetTargetPosition() + offsetPosition;
            transform.localScale = Vector3.Scale(GetTargetLocalScale(), offsetScale);
        }

        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        ParentConstraintJobHandle IJobParentConstraint.CreateJobHandle(Animator animator) {
            var handle = new ParentConstraintJobHandle();
            handle.space = _settings.space;
            handle.offsetPosition = _settings.offsetPosition;
            handle.offsetRotation = quaternion.EulerZXY(_settings.offsetAngles.z, _settings.offsetAngles.x,
                _settings.offsetAngles.y);
            handle.offsetScale = _settings.offsetScale;
            handle.ownerHandle = animator.BindStreamTransform(transform);
            handle.constraintTargetHandle = CreateTargetHandle(animator);
            return handle;
        }
    }
}