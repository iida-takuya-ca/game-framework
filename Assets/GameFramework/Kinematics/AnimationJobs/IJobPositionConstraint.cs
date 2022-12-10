using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// ジョブ対応のPositionConstraintインターフェース
    /// </summary>
    public interface IJobPositionConstraint {
        /// <summary>
        /// ジョブ要素の生成
        /// </summary>
        PositionConstraintAnimationJob.Element CreateJobElement(Animator animator);
    }
}
