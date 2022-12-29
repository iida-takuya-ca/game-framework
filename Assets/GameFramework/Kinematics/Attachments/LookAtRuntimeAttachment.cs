using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 注視追従コンポーネント
    /// </summary>
    public class LookAtRuntimeAttachment : RuntimeAttachment {
        private LookAtConstraintResolver _resolver;

        // Transform制御用インスタンス
        protected override ConstraintResolver Resolver => _resolver;
        
        // 追従設定
        public LookAtConstraintResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LookAtRuntimeAttachment(Transform owner) {
            _resolver = new LookAtConstraintResolver(owner);
        }
    }
}