using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標追従
    /// </summary>
    public class PositionAttachment : Attachment {
        [SerializeField, Tooltip("制御用設定")]
        private PositionConstraintResolver.ResolverSettings _settings = null;

        private PositionConstraintResolver _resolver;

        // 制御用設定
        public PositionConstraintResolver.ResolverSettings Settings {
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
            _resolver = new PositionConstraintResolver(transform);
        }
    }
}