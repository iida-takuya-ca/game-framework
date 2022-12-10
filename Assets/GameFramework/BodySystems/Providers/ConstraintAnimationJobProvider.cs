using System;
using GameFramework.Kinematics;
using GameFramework.MotionSystems;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Constraint用JobProvider
    /// </summary>
    public class ConstraintAnimationJobProvider : IAnimationJobProvider<ConstraintAnimationJobProvider.AnimationJob> {
        /// <summary>
        /// Job本体
        /// </summary>
        [BurstCompile]
        public struct AnimationJob : IAnimationJob {
            public NativeSlice<int> handleCounts;
            public NativeSlice<PositionConstraintJobHandle> positionConstraintJobHandles;

            /// <summary>
            /// RootMotion更新用
            /// </summary>
            void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
            }

            /// <summary>
            /// 通常のBone更新用
            /// </summary>
            void IAnimationJob.ProcessAnimation(AnimationStream stream) {
                var positionCount = handleCounts[0];
                for (var i = 0; i < positionCount; i++) {
                    positionConstraintJobHandles[i].ProcessAnimation(stream);
                }
            }
        }

        private Animator _animator;
        private int _handleCountMax;
        private NativeArray<int> _handleCounts;
        private NativeArray<PositionConstraintJobHandle> _positionConstraintJobHandles;

        // 実行優先度
        int IAnimationJobProvider.ExecutionOrder => 5;

        /// <summary>
        /// JobHandle生成に対応したPositionConstraintを設定
        /// </summary>
        public void SetConstraint(IJobPositionConstraint[] constraints) {
            ClearPositionHandles();

            var count = Mathf.Min(_handleCountMax, constraints.Length);
            _handleCounts[0] = count;
            for (var i = 0; i < count; i++) {
                _positionConstraintJobHandles[i] = constraints[i].CreateJobHandle(_animator);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handleCountMax">各種ConstraintHandleの最大数</param>
        public ConstraintAnimationJobProvider(int handleCountMax = 128) {
            _handleCountMax = handleCountMax;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        AnimationJob IAnimationJobProvider<AnimationJob>.Initialize(MotionPlayer player) {
            _animator = player.Animator;
            _handleCounts = new NativeArray<int>(1, Allocator.Persistent);
            _positionConstraintJobHandles =
                new NativeArray<PositionConstraintJobHandle>(_handleCountMax, Allocator.Persistent);

            return new AnimationJob {
                handleCounts = _handleCounts,
                positionConstraintJobHandles = _positionConstraintJobHandles,
            };
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            ClearPositionHandles();

            if (_positionConstraintJobHandles.IsCreated) {
                _positionConstraintJobHandles.Dispose();
            }

            if (_handleCounts.IsCreated) {
                _handleCounts.Dispose();
            }
        }

        /// <summary>
        /// PositionConstraintHandleの解放(配列は消さない)
        /// </summary>
        private void ClearPositionHandles() {
            if (!_handleCounts.IsCreated || !_positionConstraintJobHandles.IsCreated) {
                return;
            }

            var count = _handleCounts[0];
            for (var i = 0; i < count; i++) {
                _positionConstraintJobHandles[i].constraintAnimationJobParameter.Dispose();
            }

            _handleCounts[0] = 0;
        }
    }
}