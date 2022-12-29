using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用コンポーネント
    /// </summary>
    public class ParentRuntimeAttachment : RuntimeAttachment {
        private ParentConstraintResolver _resolver;

        // Transform制御用インスタンス
        protected override ConstraintResolver Resolver => _resolver;
        
        // 追従設定
        public ParentConstraintResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ParentRuntimeAttachment(Transform owner) {
            _resolver = new ParentConstraintResolver(owner);
        }
    }
}