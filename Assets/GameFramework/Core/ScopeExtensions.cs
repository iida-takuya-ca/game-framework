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
        public static T ScopeTo<T>(this T source, IScope scope)
            where T : IDisposable {
            scope.OnExpired += () => {
                source.Dispose();
            };
            return source;
        }

        /// <summary>
        /// GameObjectのScope登録
        /// </summary>
        public static GameObject ScopeTo(this GameObject source, IScope scope) {
            scope.OnExpired += () => {
                if (source != null) {
                    Object.Destroy(source);
                }
            };
            return source;
        }

        /// <summary>
        /// ComponentのScope登録
        /// </summary>
        public static T ScopeTo<T>(this Component source, IScope scope)
            where T : Component {
            scope.OnExpired += () => {
                if (source != null) {
                    Object.Destroy(source);
                }
            };
            return source as T;
        }
    }
}
