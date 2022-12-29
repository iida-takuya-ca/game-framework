using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 拡大縮小追従用のResolver
    /// </summary>
    public class ScaleConstraintResolver : ConstraintResolver {
        // 設定
        public class ResolverSettings {
            [Tooltip("スケールオフセット")]
            public Vector3 offsetScale = Vector3.one;
        }

        // 設定
        public ResolverSettings Settings { get; set; } = new ResolverSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象のTransform</param>
        public ScaleConstraintResolver(Transform owner)
            : base(owner) { }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void ApplyTransform() {
            Owner.localScale = Vector3.Scale(GetTargetLocalScale(), Settings.offsetScale);
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            Settings.offsetScale = Vector3.one;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            // Scale
            var targetScale = GetTargetLocalScale();
            var localScale = Owner.localScale;
            Settings.offsetScale = new Vector3
            (
                localScale.x / targetScale.x,
                localScale.y / targetScale.y,
                localScale.z / targetScale.z
            );
        }
    }
}