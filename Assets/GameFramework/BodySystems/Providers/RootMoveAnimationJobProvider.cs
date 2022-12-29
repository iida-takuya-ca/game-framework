using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using GameFramework.MotionSystems;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Rootモーションのスケールコントロール用JobProvider
    /// </summary>
    public class RootMoveAnimationJobProvider : IAnimationJobProvider<RootMoveAnimationJobProvider.AnimationJob> {
        /// <summary>
        /// Job本体
        /// </summary>
        [BurstCompile]
        public struct AnimationJob : IAnimationJob {
            [ReadOnly]
            public NativeArray<float3> vectorProperties;

            /// <summary>
            /// RootMotion更新用
            /// </summary>
            void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
                stream.velocity = stream.velocity * vectorProperties[0] + vectorProperties[2];
                stream.angularVelocity = stream.angularVelocity * vectorProperties[1] + vectorProperties[3];
            }

            /// <summary>
            /// 通常のBone更新用
            /// </summary>
            void IAnimationJob.ProcessAnimation(AnimationStream stream) {
            }
        }

        // パラメータを渡すための配列
        private NativeArray<float3> _vectorProperties;

        // 実行優先度
        int IAnimationJobProvider.ExecutionOrder => 0;

        // ルート移動のスケール
        public Vector3 PositionScale {
            get => _vectorProperties.IsCreated ? (Vector3)_vectorProperties[0] : Vector3.zero;
            set {
                if (_vectorProperties.IsCreated) {
                    _vectorProperties[0] = value;
                }
            }
        }
        
        // ルート移動速度のオフセット
        public Vector3 VelocityOffset {
            get => _vectorProperties.IsCreated ? _vectorProperties[2] : Vector3.zero;
            set {
                if (_vectorProperties.IsCreated) {
                    _vectorProperties[2] = value;
                }
            }
        }

        // ルート回転のスケール
        public Vector3 AngleScale {
            get => _vectorProperties.IsCreated ? (Vector3)_vectorProperties[1] : Vector3.zero;
            set {
                if (_vectorProperties.IsCreated) {
                    _vectorProperties[1] = value;
                }
            }
        }
        
        // ルート角速度のオフセット
        public Vector3 AngularVelocityOffset {
            get => _vectorProperties.IsCreated ? _vectorProperties[3] : Vector3.zero;
            set {
                if (_vectorProperties.IsCreated) {
                    _vectorProperties[3] = value;
                }
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        AnimationJob IAnimationJobProvider<AnimationJob>.Initialize(MotionPlayer player) {
            _vectorProperties = new NativeArray<float3>(4, Allocator.Persistent);
            _vectorProperties[0] = Vector3.one;
            _vectorProperties[1] = Vector3.one;
            _vectorProperties[2] = Vector3.zero;
            _vectorProperties[3] = Vector3.zero;

            return new AnimationJob {
                vectorProperties = _vectorProperties,
            };
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="playable">Jobを保持しているPlayable</param>
        /// <param name="deltaTime">変位時間</param>
        void IAnimationJobProvider.Update(AnimationScriptPlayable playable, float deltaTime) {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            _vectorProperties.Dispose();
        }
    }
}