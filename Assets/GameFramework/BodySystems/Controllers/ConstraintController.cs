using System.Collections.Generic;
using System.Linq;
using GameFramework.Kinematics;

namespace GameFramework.BodySystems {
    /// <summary>
    /// コンストレイント制御クラス
    /// </summary>
    public class ConstraintController : BodyController {
        // コンストレイントリスト
        private List<IExpression> _constraints = new List<IExpression>();
        // 外部から追加されたコンストレイントリスト
        private readonly List<IExpression> _customConstraints = new List<IExpression>();
        // ジョブ制御用JobProvider
        private ConstraintAnimationJobProvider _jobProvider;

        // 実行優先度
        public override int ExecutionOrder => 15;

        /// <summary>
        /// コンストレイントの追加
        /// </summary>
        public void RegisterConstraint(IExpression constraint) {
            _customConstraints.Add(constraint);
            constraint.RefreshTargets(Body.Transform);
        }

        /// <summary>
        /// コンストレイントの解除
        /// </summary>
        public void UnregisterConstraint(IExpression constraint) {
            _customConstraints.Remove(constraint);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            var meshController = Body.GetController<MeshController>();
            var motionController = Body.GetController<MotionController>();

            if (meshController != null) {
                meshController.OnRefreshed += RefreshConstraints;
            }

            if (motionController != null && motionController.UseAnimationJob) {
                _jobProvider = new ConstraintAnimationJobProvider();
                motionController.Player.AddJob(_jobProvider);
            }

            RefreshConstraints();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            // Jobで更新する場合はここでは更新しない
            if (_jobProvider != null) {
                return;
            }

            // 各種Transform更新
            foreach (var constraint in _constraints) {
                constraint.ManualUpdate();
            }

            foreach (var constraint in _customConstraints) {
                constraint.ManualUpdate();
            }
        }

        /// <summary>
        /// Constraintリストのリフレッシュ
        /// </summary>
        private void RefreshConstraints() {
            // 階層に含まれているConstraintを探す
            var bodyTransform = Body.Transform;
            _constraints = bodyTransform.GetComponentsInChildren<IExpression>(true)
                .Where(x => x != null)
                .ToList();

            var positionJobHandles = new List<IJobPositionConstraint>();
            var rotationJobHandles = new List<IJobRotationConstraint>();
            var scaleJobHandles = new List<IJobScaleConstraint>();
            var parentJobHandles = new List<IJobParentConstraint>();

            // JobConstraintの列挙
            void AddJobConstraints(IExpression constraint) {
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
            foreach (var constraint in _constraints) {
                if (constraint is ConstraintExpression c) {
                    c.UpdateMode = ConstraintExpression.Mode.Manual;
                }

                constraint.RefreshTargets(bodyTransform);
                AddJobConstraints(constraint);
            }

            foreach (var constraint in _customConstraints) {
                constraint.RefreshTargets(bodyTransform);
                AddJobConstraints(constraint);
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