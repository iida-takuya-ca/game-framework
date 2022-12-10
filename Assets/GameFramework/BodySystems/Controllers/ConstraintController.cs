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

            if (meshController != null) {
                meshController.OnRefreshed += RefreshConstraints;
            }

            RefreshConstraints();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
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
            // 骨名追従用のコンストレイントを探す
            var bodyTransform = Body.Transform;
            _constraints = bodyTransform.GetComponentsInChildren<IConstraint>(true)
                .Where(x => x != null)
                .ToList();

            foreach (var constraint in _constraints) {
                if (constraint is Constraint c) {
                    c.UpdateMode = Constraint.Mode.Manual;
                }

                constraint.RefreshTargets(bodyTransform);
            }

            foreach (var constraint in _customConstraints) {
                constraint.RefreshTargets(bodyTransform);
            }
        }
    }
}