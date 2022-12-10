using System.Collections.Generic;
using System.Linq;
using GameFramework.Kinematics;

namespace GameFramework.BodySystems {
    /// <summary>
    /// コンストレイント制御クラス
    /// </summary>
    public class ConstraintController : BodyController {
        // コンストレイントリスト
        private List<IConstraint> _constraints = new List<IConstraint>();
        // 外部から追加されたコンストレイントリスト
        private readonly List<IConstraint> _customConstraints = new List<IConstraint>();
        // ジョブ制御用JobProvider
        private ConstraintAnimationJobProvider _jobProvider;

        // 実行優先度
        public override int ExecutionOrder => 15;

        /// <summary>
        /// コンストレイントの追加
        /// </summary>
        public void RegisterConstraint(IConstraint constraint) {
            _customConstraints.Add(constraint);
            constraint.RefreshTargets(Body.Transform);
        }

        /// <summary>
        /// コンストレイントの解除
        /// </summary>
        public void UnregisterConstraint(IConstraint constraint) {
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

            if (motionController != null) {
                _jobProvider = new ConstraintAnimationJobProvider();
                motionController.Player.AddJob(_jobProvider);
            }

            RefreshConstraints();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
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
            _constraints = bodyTransform.GetComponentsInChildren<IConstraint>(true)
                .Where(x => x != null)
                .ToList();
            
            var positionJobHandles = new List<IJobPositionConstraint>();
            
            // JobConstraintの列挙
            void AddJobConstraints(IConstraint constraint) {
                if (_jobProvider == null) {
                    return;
                }
                if (constraint is IJobPositionConstraint jobPositionConstraint) {
                    positionJobHandles.Add(jobPositionConstraint);
                }
            }
            
            // ターゲット情報の更新＆JobHandleの列挙
            foreach (var constraint in _constraints) {
                if (constraint is Constraint c) {
                    c.UpdateMode = Constraint.Mode.Manual;
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
            }
        }
    }
}