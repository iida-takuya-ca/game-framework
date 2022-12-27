using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameFramework.Kinematics;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Bodyの骨制御クラス
    /// </summary>
    public class BoneController : SerializedBodyController {
        private class ExpressionInfo {
            public List<IExpression> expressions = new List<IExpression>();
        }

        [SerializeField, Tooltip("ルート骨")]
        private Transform _rootTransform = default;
        public Transform Root => _rootTransform;

        // Expressionリスト
        private List<IExpression> _expressions = new List<IExpression>();
        // 後追加用Expressionリスト
        private readonly List<IExpression> _customExpressions = new List<IExpression>();
        // 追加したMeshに含まれているExpression情報
        private Dictionary<MeshParts, ExpressionInfo> _expressionInfos =
            new Dictionary<MeshParts, ExpressionInfo>();

        // Expressionの再構築が必要か
        private bool _expressionDirty;
        // ジョブ制御用JobProvider
        private ExpressionAnimationJobProvider _jobProvider;

        // 実行優先度
        public override int ExecutionOrder => 15;

        /// <summary>
        /// Expressionの追加
        /// </summary>
        public void RegisterExpression(IExpression expression) {
            _customExpressions.Add(expression);
            _expressionDirty = true;
        }

        /// <summary>
        /// Expressionの解除
        /// </summary>
        public void UnregisterExpression(IExpression expression) {
            _customExpressions.Remove(expression);
            _expressionDirty = true;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // MeshPartsの追加/削除時にUniqueBoneにExpressionをつける
            var meshController = Body.GetController<MeshController>();

            meshController.OnAddedMeshParts += meshParts => {
                var expressionInfo = default(ExpressionInfo);

                foreach (var info in meshParts.uniqueBoneInfos) {
                    if (info.constraintMask == 0 || info.targetBones.Length <= 0) {
                        continue;
                    }

                    if (expressionInfo == null) {
                        expressionInfo = new ExpressionInfo();
                        _expressionInfos[meshParts] = expressionInfo;
                    }

                    foreach (var bone in info.targetBones) {
                        var targetBoneName = meshController.GetOriginalBoneName(bone);
                        var constraint = new ParentRuntimeConstraintExpression(bone, targetBoneName);
                        constraint.Settings.mask = (TransformMasks)info.constraintMask;
                        expressionInfo.expressions.Add(constraint);
                        RegisterExpression(constraint);
                    }
                }
            };
            
            meshController.OnRemovedMeshParts += meshParts => {
                if (_expressionInfos.TryGetValue(meshParts, out var info)) {
                    foreach (var expression in info.expressions) {
                        UnregisterExpression(expression);
                    }
                    _expressionInfos.Remove(meshParts);
                }
            };
            
            // Mesh更新時にExpressionを再構築
            meshController.OnRefreshed += RefreshExpressions;
            
            // AnimationJob用のProvider構築
            var motionController = Body.GetController<MotionController>();
            _jobProvider = new ExpressionAnimationJobProvider();
            motionController.Player.AddJob(_jobProvider);
            
            // Expression初期化
            _expressionDirty = true;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime"></param>
        protected override void UpdateInternal(float deltaTime) {
            if (_expressionDirty) {
                RefreshExpressions();
                _expressionDirty = false;
            }
        }

        /// <summary>
        /// Expressionリストのリフレッシュ
        /// </summary>
        private void RefreshExpressions() {
            // 階層に含まれているConstraintを探す
            var bodyTransform = Body.Transform;
            _expressions = bodyTransform.GetComponentsInChildren<IExpression>(true)
                .Where(x => x != null)
                .ToList();

            var positionJobHandles = new List<IJobPositionConstraint>();
            var rotationJobHandles = new List<IJobRotationConstraint>();
            var scaleJobHandles = new List<IJobScaleConstraint>();
            var parentJobHandles = new List<IJobParentConstraint>();

            // JobExpressionの列挙
            void AddJobExpression(IExpression constraint) {
                if (_jobProvider == null) {
                    return;
                }

                if (constraint is IJobPositionConstraint jobPositionConstraint) {
                    positionJobHandles.Add(jobPositionConstraint);
                }
                else if (constraint is IJobRotationConstraint jobRotationConstraint) {
                    rotationJobHandles.Add(jobRotationConstraint);
                }
                else if (constraint is IJobScaleConstraint jobScaleConstraint) {
                    scaleJobHandles.Add(jobScaleConstraint);
                }
                else if (constraint is IJobParentConstraint jobParentConstraint) {
                    parentJobHandles.Add(jobParentConstraint);
                }
            }

            // ターゲット情報の更新＆JobHandleの列挙
            foreach (var expression in _expressions) {
                AddJobExpression(expression);
            }

            foreach (var constraint in _customExpressions) {
                AddJobExpression(constraint);
            }

            // JobHandleの登録
            if (_jobProvider != null) {
                _jobProvider.SetConstraint(positionJobHandles.ToArray());
                _jobProvider.SetConstraint(rotationJobHandles.ToArray());
                _jobProvider.SetConstraint(scaleJobHandles.ToArray());
                _jobProvider.SetConstraint(parentJobHandles.ToArray());
            }
        }
    }
}