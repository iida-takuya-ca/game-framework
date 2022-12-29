using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private ParentConstraintResolver.ResolverSettings _settings = null;

        private ParentConstraintResolver _resolver;

        // 制御用設定
        public ParentConstraintResolver.ResolverSettings Settings {
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
            _resolver = new ParentConstraintResolver(transform);
        }
    }
}