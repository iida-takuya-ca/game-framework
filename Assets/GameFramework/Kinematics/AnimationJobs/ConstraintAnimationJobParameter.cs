using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// AnimationJobに渡すパラメータ
    /// </summary>
    [BurstCompile]
    public struct ConstraintAnimationJobParameter : IDisposable {
        /// <summary>
        /// AnimationJobで扱うターゲット
        /// </summary>
        [BurstCompile]
        public struct TargetInfo {
            public TransformSceneHandle targetHandle;
            public float normalizedWeight;
        }

        // ターゲット情報の配列を格納したポインタ
        public IntPtr targetInfosPtr;
        // ターゲット情報の配列要素数
        public int targetInfoCount;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public unsafe void Dispose() {
            if (targetInfosPtr != IntPtr.Zero) {
                UnsafeUtility.Free((void*)targetInfosPtr, Allocator.Persistent);
                targetInfosPtr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 追従位置取得
        /// </summary>
        public float3 GetPosition(AnimationStream stream) {
            var result = float3.zero;
            var targetInfos = GetTargetInfos();
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
            var targetInfos = GetTargetInfos();
            for (var i = 0; i < targetInfos.Length; i++) {
                result = math.mul(result,
                    math.slerp(quaternion.identity, targetInfos[i].targetHandle.GetRotation(stream),
                        targetInfos[i].normalizedWeight));
            }

            return result;
        }

        /// <summary>
        /// 追従スケール取得
        /// </summary>
        public float3 GetLocalScale(AnimationStream stream) {
            var result = float3.zero;
            var targetInfos = GetTargetInfos();
            for (var i = 0; i < targetInfos.Length; i++) {
                result += (float3)targetInfos[i].targetHandle.GetLocalScale(stream) * targetInfos[i].normalizedWeight;
            }

            return result;
        }

        /// <summary>
        /// IntPtr型からNativeArrayに変換
        /// </summary>
        private unsafe NativeArray<TargetInfo> GetTargetInfos() {
            return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TargetInfo>((void*)targetInfosPtr, targetInfoCount, Allocator.Invalid);
        }
    }
}