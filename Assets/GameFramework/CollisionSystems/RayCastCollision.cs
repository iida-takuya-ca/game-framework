using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// RayCastコリジョン
    /// </summary>
    public class RayCastCollision : Collision {
        // 結果格納最大数
        private const int ResultCountMax = 8;
        // 当たり判定受け取り用の配列
        private static RaycastHit[] s_workResults = new RaycastHit[ResultCountMax];

        // 開始位置
        public Vector3 Start { get; set; }
        // 終了位置
        public Vector3 End { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="end">終了位置</param>
        public RayCastCollision(Vector3 start, Vector3 end) {
            Start = start;
            End = end;
        }

        /// <summary>
        /// 現在のEndをStartにして、Endを進める
        /// </summary>
        /// <param name="nextEnd">新しいEnd</param>
        public void March(Vector3 nextEnd) {
            Start = End;
            End = nextEnd;
        }

        /// <summary>
        /// 当たり判定実行
        /// </summary>
        /// <param name="layerMask">衝突対象のLayerMask</param>
        /// <param name="hitResults">判定格納用配列</param>
        /// <returns>衝突有効数</returns>
        protected override int HitCheck(int layerMask, Collider[] hitResults) {
            var direction = End - Start;
            var distance = direction.magnitude;
            var count = Physics.RaycastNonAlloc(Start, direction, s_workResults, distance, layerMask);
            for (var i = 0; i < count; i++) {
                if (i >= hitResults.Length) {
                    break;
                }

                hitResults[i] = s_workResults[i].collider;
            }

            return count;
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        protected override void DrawGizmosInternal() {
            Gizmos.DrawLine(Start, End);
        }
    }
}