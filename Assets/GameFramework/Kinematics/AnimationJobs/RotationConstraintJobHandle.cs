using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 回転追従要素
    /// </summary>
    [BurstCompile]
    public struct RotationConstraintJobHandle : IDisposable {
        public TransformStreamHandle ownerHandle;
        [ReadOnly]
        public Space space;
        [ReadOnly]
        public quaternion offsetRotation;
        [ReadOnly]
        public ConstraintTargetHandle constraintTargetHandle;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            constraintTargetHandle.Dispose();
        }

        /// <summary>
        /// Bone更新
        /// </summary>
        public void ProcessAnimation(AnimationStream stream) {
            var rotation = offsetRotation;
            if (space == Space.Self) {
                rotation = math.mul(constraintTargetHandle.GetRotation(stream), rotation);
            }
            else {
                rotation = math.mul(rotation, constraintTargetHandle.GetRotation(stream));
            }

            ownerHandle.SetRotation(stream, rotation);
        }
    }
}