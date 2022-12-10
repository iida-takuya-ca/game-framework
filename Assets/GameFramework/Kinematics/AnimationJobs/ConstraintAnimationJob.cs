using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標追従要素
    /// </summary>
    [BurstCompile]
    public struct PositionConstraintJobHandle : IDisposable {
        public TransformStreamHandle ownerHandle;
        [ReadOnly]
        public Space space;
        [ReadOnly]
        public float3 offsetPosition;
        [ReadOnly]
        public ConstraintAnimationJobParameter constraintAnimationJobParameter;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            constraintAnimationJobParameter.Dispose();
        }

        /// <summary>
        /// Bone更新
        /// </summary>
        public void ProcessAnimation(AnimationStream stream) {
            var offset = offsetPosition;
            if (space == Space.Self) {
                ownerHandle.GetGlobalTR(stream, out var _, out var rot);
                offset = math.mul(rot, offset);
            }
                
            ownerHandle.SetPosition(stream, constraintAnimationJobParameter.GetPosition(stream) + offset);
        }
    }
}
