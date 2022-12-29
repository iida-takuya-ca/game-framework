using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 姿勢追従
    /// </summary>
    public class RotationRuntimeAttachment : RuntimeAttachment {
        private RotationConstraintResolver _resolver;

        // Transform制御用インスタンス
        protected override ConstraintResolver Resolver => _resolver;
        
        // 追従設定
        public RotationConstraintResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RotationRuntimeAttachment(Transform owner) {
            _resolver = new RotationConstraintResolver(owner);
        }
    }
}