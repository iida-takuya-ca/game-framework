using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// ParentConstraintのJob実行用処理
    /// </summary>
    [BurstCompile]
    public struct ParentConstraintJobHandle : IDisposable {
        public TransformStreamHandle ownerHandle;
        [ReadOnly]
        public Space space;
        [ReadOnly]
        public float3 offsetPosition;
        [ReadOnly]
        public quaternion offsetRotation;
        [ReadOnly]
        public float3 offsetScale;
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
            var ofsPos = offsetPosition;
            var rot = offsetRotation;
            var ofsScale = offsetScale;
            if (space == Space.Self) {
                ownerHandle.GetGlobalTR(stream, out var _, out var r);
                ofsPos = math.mul(r, ofsPos);
                rot = math.mul(constraintTargetHandle.GetRotation(stream), rot);
            }
            else {
                rot = math.mul(rot, constraintTargetHandle.GetRotation(stream));
            }

            ownerHandle.SetPosition(stream, constraintTargetHandle.GetPosition(stream) + ofsPos);
            ownerHandle.SetRotation(stream, rot);
            ownerHandle.SetLocalScale(stream, constraintTargetHandle.GetLocalScale(stream) * ofsScale);
        }
    }
}