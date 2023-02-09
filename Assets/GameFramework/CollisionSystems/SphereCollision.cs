using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 球体コリジョン
    /// </summary>
    public class SphereCollision : Collision {
        // 中心座標
        public Vector3 Center { get; set; }
        // 半径
        public float Radius { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        public SphereCollision(Vector3 center, float radius) {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// 当たり判定実行
        /// </summary>
        /// <param name="layerMask">衝突対象のLayerMask</param>
        /// <param name="hitResults">判定格納用配列</param>
        /// <returns>衝突有効数</returns>
        protected override int HitCheck(int layerMask, Collider[] hitResults) {
            return Physics.OverlapSphereNonAlloc(Center, Radius, hitResults, layerMask);
        }
    }
}