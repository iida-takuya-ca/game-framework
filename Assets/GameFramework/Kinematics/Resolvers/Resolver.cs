using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用Resolver
    /// </summary>
    public class ParentConstraintResolver {
        // 設定
        [Serializable]
        public class ResolverSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition = Vector3.zero;
            public Vector3 offsetAngles = Vector3.zero;
            public Vector3 offsetScale = Vector3.one;
            public TransformMasks mask = KinematicsDefinitions.TransformMasksAll;
        }

        // 設定
        public ResolverSettings Settings { get; set; } = new ResolverSettings();

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public void ResetOffset() {
            Settings.offsetPosition = Vector3.zero;
            Settings.offsetAngles = Vector3.zero;
            Settings.offsetScale = Vector3.one;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public void TransferOffset(Transform owner) {
            var space = Settings.space;
            // Position
            var offsetPosition = owner.position - GetTargetPosition();

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
            var rotation = Quaternion.Euler(_settings.offsetAngles);
            var offsetScale = _settings.offsetScale;

            if (space == Space.Self) {
                offsetPosition = transform.TransformVector(offsetPosition);
                rotation = GetTargetRotation() * rotation;
            }
            else {
                rotation = rotation * GetTargetRotation();
            }

            if ((_settings.mask & TransformMasks.Position) != 0) {
                transform.position = GetTargetPosition() + offsetPosition;
            }

            if ((_settings.mask & TransformMasks.Rotation) != 0) {
                transform.rotation = rotation;
            }

            if ((_settings.mask & TransformMasks.Scale) != 0) {
                transform.localScale = Vector3.Scale(GetTargetLocalScale(), offsetScale);
            }
        }
    }
}