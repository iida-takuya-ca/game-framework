using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.MotionSystems {
    /// <summary>
    /// Constraint用JobProvider
    /// </summary>
    // public class ConstraintAnimationJobProvider : IAnimationJobProvider<ConstraintAnimationJobProvider.RootScaleAnimationJob> {
    //     /// <summary>
    //     /// Job本体
    //     /// </summary>
    //     [BurstCompile]
    //     public struct AnimationJob : IAnimationJob {
    //         [ReadOnly]
    //         public NativeArray<TransformSceneHandle> vectorProperties;
    //
    //         /// <summary>
    //         /// RootMotion更新用
    //         /// </summary>
    //         void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
    //             stream.velocity *= vectorProperties[0];
    //             stream.angularVelocity *= vectorProperties[1];
    //         }
    //
    //         /// <summary>
    //         /// 通常のBone更新用
    //         /// </summary>
    //         void IAnimationJob.ProcessAnimation(AnimationStream stream) {
    //         }
    //     }
    //
    //     // パラメータを渡すための配列
    //     private NativeArray<float3> _vectorProperties;
    //
    //     // 実行優先度
    //     int IAnimationJobProvider<RootScaleAnimationJob>.ExecutionOrder => 0;
    //
    //     // ルート移動のスケール
    //     public Vector3 PositionScale {
    //         get => _vectorProperties.IsCreated ? (Vector3)_vectorProperties[0] : Vector3.zero;
    //         set {
    //             if (_vectorProperties.IsCreated) {
    //                 _vectorProperties[0] = value;
    //             }
    //         }
    //     }
    //
    //     // ルート回転のスケール
    //     public Vector3 AngleScale {
    //         get => _vectorProperties.IsCreated ? (Vector3)_vectorProperties[1] : Vector3.zero;
    //         set {
    //             if (_vectorProperties.IsCreated) {
    //                 _vectorProperties[1] = value;
    //             }
    //         }
    //     }
    //
    //     /// <summary>
    //     /// 初期化処理
    //     /// </summary>
    //     RootScaleAnimationJob IAnimationJobProvider<RootScaleAnimationJob>.Initialize(MotionPlayer player) {
    //         _vectorProperties = new NativeArray<float3>(2, Allocator.Persistent);
    //         _vectorProperties[0] = Vector3.one;
    //         _vectorProperties[1] = Vector3.one;
    //
    //         return new RootScaleAnimationJob {
    //             vectorProperties = _vectorProperties,
    //         };
    //     }
    //
    //     /// <summary>
    //     /// 廃棄時処理
    //     /// </summary>
    //     void IDisposable.Dispose() {
    //         _vectorProperties.Dispose();
    //     }
    // }
}