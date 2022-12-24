using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 回転追従要素
    /// </summary>
    [BurstCompile]
    public struct ScaleConstraintJobHandle : IDisposable {
        public TransformStreamHandle ownerHandle;
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
            ownerHandle.SetLocalScale(stream, constraintTargetHandle.GetLocalScale(stream) * offsetScale);
        }
    }
}