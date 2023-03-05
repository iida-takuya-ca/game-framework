using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 衝突結果
    /// </summary>
    public struct HitResult {
        // 衝突検知したCollider
        public Collider collider;
        // カスタムデータ
        public object customData;
    }
}