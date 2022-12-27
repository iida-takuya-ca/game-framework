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
            public TransformMasks mask = KinematicsDefinitions.TransformMasksAll;
        }

        [SerializeField, Tooltip("コンストレイント設定")]
        private ConstraintSettings _settings = null;

        // コンストレイント設定
        public ConstraintSettings Settings {
            set { _settings = value; }
        }

        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        ParentConstraintJobHandle IJobParentConstraint.CreateJobHandle(Animator animator) {
            var handle = new ParentConstraintJobHandle();
            handle.space = _settings.space;
            handle.masks = _settings.mask;
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