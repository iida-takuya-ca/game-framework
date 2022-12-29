using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// エイム追従コンポーネント
    /// </summary>
    public class AimAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private AimConstraintResolver.ResolverSettings _settings = null;

        private AimConstraintResolver _resolver;

        // 制御用設定
        public AimConstraintResolver.ResolverSettings Settings {
            set {
                _settings = value;
                _resolver.Settings = _settings;
            }
        }
        // Transform制御用クラス
        protected override ConstraintResolver Resolver => _resolver;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void Initialize() {
            _resolver = new AimConstraintResolver(transform);
        }
    }
}