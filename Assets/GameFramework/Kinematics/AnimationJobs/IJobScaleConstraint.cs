using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// ジョブ対応のScaleConstraintインターフェース
    /// </summary>
    public interface IJobScaleConstraint {
        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        ScaleConstraintJobHandle CreateJobHandle(Animator animator);
    }
}