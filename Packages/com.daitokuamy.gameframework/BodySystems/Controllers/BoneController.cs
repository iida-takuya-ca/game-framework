using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Bodyの骨制御クラス
    /// </summary>
    public class BoneController : SerializedBodyController {
        /// <summary>
        /// Job本体
        /// </summary>
        [BurstCompile]
        public struct AnimationJob : IAnimationJob {
            [ReadOnly]
            public NativeArray<TransformStreamHandle> sourceTransformHandles;
            [ReadOnly]
            public NativeArray<MeshParts.ConstraintMasks> constraintMasksList;
            [WriteOnly]
            public NativeArray<TransformStreamHandle> destinationTransformHandles;

            /// <summary>
            /// RootMotion更新用
            /// </summary>
            void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
            }

            /// <summary>
            /// 通常のBone更新用
            /// </summary>
            void IAnimationJob.ProcessAnimation(AnimationStream stream) {
                for (var i = 0; i < sourceTransformHandles.Length; i++) {
                    var masks = constraintMasksList[i];
                    if ((masks & MeshParts.ConstraintMasks.Position) != 0) {
                        var position = sourceTransformHandles[i].GetLocalPosition(stream);
                        destinationTransformHandles[i].SetLocalPosition(stream, position);
                    }

                    if ((masks & MeshParts.ConstraintMasks.Rotation) != 0) {
                        var rotation = sourceTransformHandles[i].GetLocalRotation(stream);
                        destinationTransformHandles[i].SetLocalRotation(stream, rotation);
                    }

                    if ((masks & MeshParts.ConstraintMasks.LocalScale) != 0) {
                        var localScale = sourceTransformHandles[i].GetLocalScale(stream);
                        destinationTransformHandles[i].SetLocalScale(stream, localScale);
                    }
                }
            }
        }

        [SerializeField, Tooltip("骨検索のためのルートとなるTransform")]
        private Transform _boneRoot = default;
        [SerializeField, Tooltip("AnimationJob登録時の実行オーダー")]
        private ushort _jobSortingOrder = 500;

        // Animator
        private Animator _animator;
        // Mesh制御用クラス
        private MeshController _meshController;

        // Constraintの更新フラグ
        private bool _constraintDirty;

        // Constraint用Graph情報
        private PlayableGraph _graph;
        private AnimationScriptPlayable _playable;

        // パラメータを渡すための配列
        private NativeArray<TransformStreamHandle> _sourceTransformHandles;
        private NativeArray<MeshParts.ConstraintMasks> _constraintMasksList;
        private NativeArray<TransformStreamHandle> _destinationTransformHandles;

        // BoneのRootTransform
        public Transform BoneRoot => _boneRoot;

        // 実行優先度
        public override int ExecutionOrder => 15;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _animator = Body.GetComponent<Animator>();
            _meshController = Body.GetController<MeshController>();

            if (_meshController != null) {
                _meshController.OnAddedMeshParts += _ => _constraintDirty = true;
                _meshController.OnRemovedMeshParts += _ => _constraintDirty = true;
            }

            _constraintDirty = true;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            ClearGraph();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            if (_constraintDirty) {
                _constraintDirty = false;
                BuildGraph();

                // AnimatorのRebind
                if (_animator != null) {
                    _animator.Rebind();
                }
            }
        }

        /// <summary>
        /// グラフの構築
        /// </summary>
        private void BuildGraph() {
            ClearGraph();

            if (_meshController == null) {
                return;
            }

            // 追従対象の骨を列挙
            var partsList = _meshController.GetMeshPartsList();
            var constraintInfos = new List<(Transform, Transform, MeshParts.ConstraintMasks)>();
            foreach (var parts in partsList) {
                foreach (var info in parts.uniqueBoneInfos) {
                    if (info.constraintMask == 0) {
                        continue;
                    }

                    foreach (var bone in info.targetBones) {
                        if (bone == null) {
                            continue;
                        }

                        var originalBone = _meshController.GetOriginalBone(bone);
                        if (originalBone == null) {
                            continue;
                        }

                        constraintInfos.Add((originalBone, bone, info.constraintMask));
                    }
                }
            }

            if (constraintInfos.Count <= 0) {
                return;
            }

            // Graph/Outputの生成
            _graph = PlayableGraph.Create($"{nameof(BoneController)}({Body.Transform.name})");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Output", _animator);
            output.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
            output.SetSortingOrder(_jobSortingOrder);

            // バッファの構築
            _sourceTransformHandles = new NativeArray<TransformStreamHandle>(constraintInfos.Count, Allocator.Persistent);
            _destinationTransformHandles = new NativeArray<TransformStreamHandle>(constraintInfos.Count, Allocator.Persistent);
            _constraintMasksList = new NativeArray<MeshParts.ConstraintMasks>(constraintInfos.Count, Allocator.Persistent);
            for (var i = 0; i < constraintInfos.Count; i++) {
                _sourceTransformHandles[i] = _animator.BindStreamTransform(constraintInfos[i].Item1);
                _destinationTransformHandles[i] = _animator.BindStreamTransform(constraintInfos[i].Item2);
                _constraintMasksList[i] = constraintInfos[i].Item3;
            }

            // Playableの生成
            var job = new AnimationJob {
                sourceTransformHandles = _sourceTransformHandles,
                destinationTransformHandles = _destinationTransformHandles,
                constraintMasksList = _constraintMasksList
            };

            _playable = AnimationScriptPlayable.Create(_graph, job);
            output.SetSourcePlayable(_playable);
        }

        /// <summary>
        /// グラフの削除
        /// </summary>
        private void ClearGraph() {
            _animator.UnbindAllStreamHandles();

            if (_sourceTransformHandles.IsCreated) {
                _sourceTransformHandles.Dispose();
            }

            if (_destinationTransformHandles.IsCreated) {
                _destinationTransformHandles.Dispose();
            }

            if (_constraintMasksList.IsCreated) {
                _constraintMasksList.Dispose();
            }

            if (_playable.IsValid()) {
                _playable.Destroy();
            }

            if (_graph.IsValid()) {
                _graph.Destroy();
            }
        }
    }
}