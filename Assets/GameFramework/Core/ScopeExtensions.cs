using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Core {
    /// <summary>
    /// IScope用の拡張メソッド
    /// </summary>
    public static class ScopeExtensions {
        /// <summary>
        /// IDisposableのScope登録
        /// </summary>
        public static T ScopeTo<T>(this T self, IScope scope)
            where T : IDisposable {
            scope.OnExpired += () => {
                self.Dispose();
            };
            return self;
        }

        /// <summary>
        /// GameObjectのScope登録
        /// </summary>
        public static GameObject ScopeTo(this GameObject self, IScope scope) {
            scope.OnExpired += () => {
                if (self != null) {
                    Object.Destroy(self);
                }
            };
            return self;
        }

        /// <summary>
        /// ComponentのScope登録
        /// </summary>
        public static T ScopeTo<T>(this Component self, IScope scope)
            where T : Component {
            scope.OnExpired += () => {
                if (self != null) {
                    Object.Destroy(self);
                }
            };
            return self as T;
        }
    }
}
