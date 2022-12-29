using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 拡大縮小追従
    /// </summary>
    public class ScaleRuntimeAttachment : RuntimeAttachment {
        private ScaleConstraintResolver _resolver;

        // Transform制御用インスタンス
        protected override ConstraintResolver Resolver => _resolver;
        
        // 追従設定
        public ScaleConstraintResolver.ResolverSettings Settings {
            get => _resolver.Settings;
            set => _resolver.Settings = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScaleRuntimeAttachment(Transform owner) {
            _resolver = new ScaleConstraintResolver(owner);
        }
    }
}