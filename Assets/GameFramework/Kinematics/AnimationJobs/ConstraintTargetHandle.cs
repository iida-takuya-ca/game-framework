using System;
using MacFsWatcher;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// AnimationJobに渡すパラメータ
    /// </summary>
    [BurstCompile]
    public unsafe struct ConstraintTargetHandle : IDisposable {
        /// <summary>
        /// AnimationJobで扱うターゲット
        /// </summary>
        [BurstCompile]
        public struct TargetInfo {
            public TransformSceneHandle targetHandle;
            public float normalizedWeight;
        }

        // ターゲット情報の配列を格納したポインタ
        public TargetInfo* targetInfosPtr;
        // ターゲット情報の配列要素数
        public int targetInfosCount;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (targetInfosPtr != null) {
                UnsafeUtility.Free(targetInfosPtr, Allocator.Persistent);
                targetInfosPtr = null;
            }
        }

        /// <summary>
        /// 追従位置取得
        /// </summary>
        public float3 GetPosition(AnimationStream stream) {
            var result = float3.zero;
            for (var i = 0; i < targetInfosCount; i++) {
                var targetInfo = *(targetInfosPtr + i);
                result += (float3)targetInfo.targetHandle.GetPosition(stream) * targetInfo.normalizedWeight;
            }

            return result;
        }

        /// <summary>
        /// 追従向き取得
        /// </summary>
        public quaternion GetRotation(AnimationStream stream) {
            var result = quaternion.identity;
            for (var i = 0; i < targetInfosCount; i++) {
                var targetInfo = *(targetInfosPtr + i);
                result = math.mul(result,
                    math.slerp(quaternion.identity, targetInfo.targetHandle.GetRotation(stream),
                        targetInfo.normalizedWeight));
            }

            return result;
        }

        /// <summary>
        /// 追従スケール取得
        /// </summary>
        public float3 GetLocalScale(AnimationStream stream) {
            var result = float3.zero;
            for (var i = 0; i < targetInfosCount; i++) {
                var targetInfo = *(targetInfosPtr + i);
                result += (float3)targetInfo.targetHandle.GetLocalScale(stream) * targetInfo.normalizedWeight;
            }

            return result;
        }

        /// <summary>
        /// TargetInfo配列情報の構築
        /// </summary>
        public void CreateTargetInfos(int length) {
            targetInfosPtr = (TargetInfo*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<TargetInfo>() * length,
                UnsafeUtility.AlignOf<TargetInfo>(), Allocator.Persistent);
            targetInfosCount = length;
        }

        /// <summary>
        /// TargetInfoの取得
        /// </summary>
        public TargetInfo GetTargetInfo(int index) {
            return *(targetInfosPtr + index);
        }

        /// <summary>
        /// TargetInfoの設定
        /// </summary>
        public void SetTargetInfo(int index, TargetInfo targetInfo) {
            *(targetInfosPtr + index) = targetInfo;
        }
    }
}