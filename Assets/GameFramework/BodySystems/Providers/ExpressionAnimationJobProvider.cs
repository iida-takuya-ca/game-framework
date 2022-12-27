using System;
using GameFramework.Kinematics;
using GameFramework.MotionSystems;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Expression用JobProvider
    /// </summary>
    public class ExpressionAnimationJobProvider : IAnimationJobProvider<ExpressionAnimationJobProvider.AnimationJob> {
        /// <summary>
        /// Countsアクセスする際のIndex
        /// </summary>
        private enum CountIndex {
            Position,
            Rotation,
            Scale,
            Parent,
        }

        /// <summary>
        /// Job本体
        /// </summary>
        [BurstCompile]
        public struct AnimationJob : IAnimationJob {
            public NativeSlice<int> handleCounts;
            public NativeSlice<PositionConstraintJobHandle> positionConstraintJobHandles;
            public NativeSlice<RotationConstraintJobHandle> rotationConstraintJobHandles;
            public NativeSlice<ScaleConstraintJobHandle> scaleConstraintJobHandles;
            public NativeSlice<ParentConstraintJobHandle> parentConstraintJobHandles;

            /// <summary>
            /// RootMotion更新用
            /// </summary>
            void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
            }

            /// <summary>
            /// 通常のBone更新用
            /// </summary>
            void IAnimationJob.ProcessAnimation(AnimationStream stream) {
                var positionCount = handleCounts[(int)CountIndex.Position];
                for (var i = 0; i < positionCount; i++) {
                    positionConstraintJobHandles[i].ProcessAnimation(stream);
                }

                var rotationCount = handleCounts[(int)CountIndex.Rotation];
                for (var i = 0; i < rotationCount; i++) {
                    rotationConstraintJobHandles[i].ProcessAnimation(stream);
                }

                var scaleCount = handleCounts[(int)CountIndex.Scale];
                for (var i = 0; i < scaleCount; i++) {
                    scaleConstraintJobHandles[i].ProcessAnimation(stream);
                }

                var parentCount = handleCounts[(int)CountIndex.Parent];
                for (var i = 0; i < parentCount; i++) {
                    parentConstraintJobHandles[i].ProcessAnimation(stream);
                }
            }
        }

        private Animator _animator;
        private int _handleCountMax;
        private NativeArray<int> _handleCounts;
        private NativeArray<PositionConstraintJobHandle> _positionConstraintJobHandles;
        private NativeArray<RotationConstraintJobHandle> _rotationConstraintJobHandles;
        private NativeArray<ScaleConstraintJobHandle> _scaleConstraintJobHandles;
        private NativeArray<ParentConstraintJobHandle> _parentConstraintJobHandles;

        // 実行優先度
        int IAnimationJobProvider.ExecutionOrder => -5;

        /// <summary>
        /// JobHandle生成に対応したPositionConstraintを設定
        /// </summary>
        public void SetConstraint(IJobPositionConstraint[] constraints) {
            ClearPositionHandles();

            var count = Mathf.Min(_handleCountMax, constraints.Length);
            _handleCounts[(int)CountIndex.Position] = count;
            for (var i = 0; i < count; i++) {
                _positionConstraintJobHandles[i] = constraints[i].CreateJobHandle(_animator);
            }
        }

        /// <summary>
        /// JobHandle生成に対応したRotationConstraintを設定
        /// </summary>
        public void SetConstraint(IJobRotationConstraint[] constraints) {
            ClearRotationHandles();

            var count = Mathf.Min(_handleCountMax, constraints.Length);
            _handleCounts[(int)CountIndex.Rotation] = count;
            for (var i = 0; i < count; i++) {
                _rotationConstraintJobHandles[i] = constraints[i].CreateJobHandle(_animator);
            }
        }

        /// <summary>
        /// JobHandle生成に対応したScaleConstraintを設定
        /// </summary>
        public void SetConstraint(IJobScaleConstraint[] constraints) {
            ClearScaleHandles();

            var count = Mathf.Min(_handleCountMax, constraints.Length);
            _handleCounts[(int)CountIndex.Scale] = count;
            for (var i = 0; i < count; i++) {
                _scaleConstraintJobHandles[i] = constraints[i].CreateJobHandle(_animator);
            }
        }

        /// <summary>
        /// JobHandle生成に対応したParentConstraintを設定
        /// </summary>
        public void SetConstraint(IJobParentConstraint[] constraints) {
            ClearParentHandles();

            var count = Mathf.Min(_handleCountMax, constraints.Length);
            _handleCounts[(int)CountIndex.Parent] = count;
            for (var i = 0; i < count; i++) {
                _parentConstraintJobHandles[i] = constraints[i].CreateJobHandle(_animator);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handleCountMax">各種ConstraintHandleの最大数</param>
        public ExpressionAnimationJobProvider(int handleCountMax = 128) {
            _handleCountMax = handleCountMax;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        AnimationJob IAnimationJobProvider<AnimationJob>.Initialize(MotionPlayer player) {
            _animator = player.Animator;
            _handleCounts = new NativeArray<int>(Enum.GetNames(typeof(CountIndex)).Length, Allocator.Persistent);
            _positionConstraintJobHandles =
                new NativeArray<PositionConstraintJobHandle>(_handleCountMax, Allocator.Persistent);
            _rotationConstraintJobHandles =
                new NativeArray<RotationConstraintJobHandle>(_handleCountMax, Allocator.Persistent);
            _scaleConstraintJobHandles =
                new NativeArray<ScaleConstraintJobHandle>(_handleCountMax, Allocator.Persistent);
            _parentConstraintJobHandles =
                new NativeArray<ParentConstraintJobHandle>(_handleCountMax, Allocator.Persistent);

            return new AnimationJob {
                handleCounts = _handleCounts,
                positionConstraintJobHandles = _positionConstraintJobHandles,
                rotationConstraintJobHandles = _rotationConstraintJobHandles,
                scaleConstraintJobHandles = _scaleConstraintJobHandles,
                parentConstraintJobHandles = _parentConstraintJobHandles,
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
            ClearPositionHandles();
            ClearRotationHandles();
            ClearScaleHandles();
            ClearParentHandles();

            if (_positionConstraintJobHandles.IsCreated) {
                _positionConstraintJobHandles.Dispose();
            }

            if (_rotationConstraintJobHandles.IsCreated) {
                _rotationConstraintJobHandles.Dispose();
            }

            if (_scaleConstraintJobHandles.IsCreated) {
                _scaleConstraintJobHandles.Dispose();
            }

            if (_parentConstraintJobHandles.IsCreated) {
                _parentConstraintJobHandles.Dispose();
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

            var count = _handleCounts[(int)CountIndex.Position];
            for (var i = 0; i < count; i++) {
                _positionConstraintJobHandles[i].constraintTargetHandle.Dispose();
            }

            _handleCounts[(int)CountIndex.Position] = 0;
        }

        /// <summary>
        /// RotationConstraintHandleの解放(配列は消さない)
        /// </summary>
        private void ClearRotationHandles() {
            if (!_handleCounts.IsCreated || !_rotationConstraintJobHandles.IsCreated) {
                return;
            }

            var count = _handleCounts[(int)CountIndex.Rotation];
            for (var i = 0; i < count; i++) {
                _rotationConstraintJobHandles[i].constraintTargetHandle.Dispose();
            }

            _handleCounts[(int)CountIndex.Rotation] = 0;
        }

        /// <summary>
        /// ScaleConstraintHandleの解放(配列は消さない)
        /// </summary>
        private void ClearScaleHandles() {
            if (!_handleCounts.IsCreated || !_scaleConstraintJobHandles.IsCreated) {
                return;
            }

            var count = _handleCounts[(int)CountIndex.Scale];
            for (var i = 0; i < count; i++) {
                _scaleConstraintJobHandles[i].constraintTargetHandle.Dispose();
            }

            _handleCounts[(int)CountIndex.Scale] = 0;
        }

        /// <summary>
        /// ParentConstraintHandleの解放(配列は消さない)
        /// </summary>
        private void ClearParentHandles() {
            if (!_handleCounts.IsCreated || !_parentConstraintJobHandles.IsCreated) {
                return;
            }

            var count = _handleCounts[(int)CountIndex.Parent];
            for (var i = 0; i < count; i++) {
                _parentConstraintJobHandles[i].constraintTargetHandle.Dispose();
            }

            _handleCounts[(int)CountIndex.Parent] = 0;
        }
    }
}