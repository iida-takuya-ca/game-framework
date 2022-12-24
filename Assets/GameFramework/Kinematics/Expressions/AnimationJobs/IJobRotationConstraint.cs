using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// ジョブ対応のRotationConstraintインターフェース
    /// </summary>
    public interface IJobRotationConstraint {
        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        RotationConstraintJobHandle CreateJobHandle(Animator animator);
    }
}