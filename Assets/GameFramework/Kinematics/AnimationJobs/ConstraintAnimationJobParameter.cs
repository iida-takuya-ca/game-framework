using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// AnimationJobに渡すパラメータ
    /// </summary>
    public struct ConstraintAnimationJobParameter : IDisposable {
        /// <summary>
        /// AnimationJobで扱うターゲット
        /// </summary>
        public struct TargetInfo {
            public TransformSceneHandle targetHandle;
            public float normalizedWeight;
        }
        
        // 追従対象のTransform
        public NativeArray<TargetInfo> targetInfos;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (!targetInfos.IsCreated) {
                return;
            }

            targetInfos.Dispose();
        }

        /// <summary>
        /// 追従位置取得
        /// </summary>
        public float3 GetPosition(AnimationStream stream) {
            var result = float3.zero;
            for (var i = 0; i < targetInfos.Length; i++) {
                result += (float3)targetInfos[i].targetHandle.GetPosition(stream) * targetInfos[i].normalizedWeight;
            }

            return result;
        }
        
        /// <summary>
        /// 追従向き取得
        /// </summary>
        public quaternion GetRotation(AnimationStream stream) {
            var result = quaternion.identity;
            for (var i = 0; i < targetInfos.Length; i++) {
                result = math.mul(result, math.slerp(quaternion.identity, targetInfos[i].targetHandle.GetRotation(stream), targetInfos[i].normalizedWeight));
            }

            return result;
        }
        
        /// <summary>
        /// 追従スケール取得
        /// </summary>
        public float3 GetLocalScale(AnimationStream stream) {
            var result = float3.zero;
            for (var i = 0; i < targetInfos.Length; i++) {
                result += (float3)targetInfos[i].targetHandle.GetLocalScale(stream) * targetInfos[i].normalizedWeight;
            }

            return result;
        }
    }
}
