using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// コンストレイント用のInterface
    /// </summary>
    public interface IConstraint {
        /// <summary>
        /// 更新処理
        /// </summary>
        void ManualUpdate();

        /// <summary>
        /// ターゲット情報の更新
        /// </summary>
        void RefreshTargets(Transform root);
    }
}