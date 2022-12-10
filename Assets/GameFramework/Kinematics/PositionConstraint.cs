using System;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標コンストレイント
    /// </summary>
    public class PositionConstraint : Constraint, IJobPositionConstraint {
        // コンストレイント設定
        [Serializable]
        public class ConstraintSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition;
        }

        [SerializeField, Tooltip("コンストレイント設定")]
        private ConstraintSettings _settings = null;

        // コンストレイント設定
        public ConstraintSettings Settings {
            get => _settings;
            set => _settings = value;
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
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            _settings.offsetPosition = Vector3.zero;
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            var space = _settings.space;
            var offset = _settings.offsetPosition;

            if (space == Space.Self) {
                offset = transform.TransformVector(offset);
            }

            transform.position = GetTargetPosition() + offset;
        }

        /// <summary>
        /// ジョブ要素の生成
        /// </summary>
        PositionConstraintAnimationJob.Element IJobPositionConstraint.CreateJobElement(Animator animator) {
            var jobElement = new PositionConstraintAnimationJob.Element();
            jobElement.space = Settings.space;
            jobElement.offsetPosition = Settings.offsetPosition;
            jobElement.ownerHandle = animator.BindStreamTransform(transform);
            jobElement.constraintAnimationJobParameter = CreateJobParameter(animator);
            return jobElement;
        }
    }
}