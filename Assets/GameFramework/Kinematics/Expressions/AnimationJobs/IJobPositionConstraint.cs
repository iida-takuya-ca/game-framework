using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// ジョブ対応のPositionConstraintインターフェース
    /// </summary>
    public interface IJobPositionConstraint {
        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        PositionConstraintJobHandle CreateJobHandle(Animator animator);
    }
}