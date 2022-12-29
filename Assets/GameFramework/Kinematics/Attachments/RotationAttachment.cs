using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 姿勢追従
    /// </summary>
    public class RotationAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private RotationConstraintResolver.ResolverSettings _settings = null;

        private RotationConstraintResolver _resolver;

        // 制御用設定
        public RotationConstraintResolver.ResolverSettings Settings {
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
            _resolver = new RotationConstraintResolver(transform);
        }
    }
}