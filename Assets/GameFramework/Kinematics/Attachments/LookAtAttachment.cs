using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 注視追従コンポーネント
    /// </summary>
    public class LookAtAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private LookAtConstraintResolver.ResolverSettings _settings = null;

        private LookAtConstraintResolver _resolver;

        // 制御用設定
        public LookAtConstraintResolver.ResolverSettings Settings {
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
            _resolver = new LookAtConstraintResolver(transform);
        }
    }
}