using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// レイキャストコリジョン用インターフェース
    /// </summary>
    public interface IRaycastCollision : IVisualizable {
        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="layerMask">当たり判定の対象とするLayerMask</param>
        /// <param name="newHitResults">新しくヒットしたRaycastHit結果格納用リスト</param>
        bool Tick(int layerMask, List<RaycastHit> newHitResults);

        /// <summary>
        /// 衝突履歴のクリア
        /// </summary>
        void ClearHistory();
    }
}