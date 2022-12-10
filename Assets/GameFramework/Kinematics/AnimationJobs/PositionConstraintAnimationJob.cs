using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Job本体
    /// </summary>
    [BurstCompile]
    public struct PositionConstraintAnimationJob : IAnimationJob, IDisposable {
        /// <summary>
        /// 骨更新要素
        /// </summary>
        [BurstCompile]
        public struct Element {
            public TransformStreamHandle ownerHandle;
            [ReadOnly]
            public Space space;
            [ReadOnly]
            public float3 offsetPosition;
            [ReadOnly]
            public ConstraintAnimationJobParameter constraintAnimationJobParameter;

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

        public NativeArray<Element> elements;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (!elements.IsCreated) {
                return;
            }

            for (var i = 0; i < elements.Length; i++) {
                elements[i].constraintAnimationJobParameter.Dispose();
            }

            elements.Dispose();
        }

        /// <summary>
        /// RootMotion更新用
        /// </summary>
        void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
        }

        /// <summary>
        /// 通常のBone更新用
        /// </summary>
        void IAnimationJob.ProcessAnimation(AnimationStream stream) {
            for (var i = 0; i < elements.Length; i++) {
                elements[i].ProcessAnimation(stream);
            }
        }
    }
}
