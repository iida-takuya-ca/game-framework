using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// ジョブ対応のParentConstraintインターフェース
    /// </summary>
    public interface IJobParentConstraint {
        /// <summary>
        /// ジョブハンドルの生成
        /// </summary>
        ParentConstraintJobHandle CreateJobHandle(Animator animator);
    }
}