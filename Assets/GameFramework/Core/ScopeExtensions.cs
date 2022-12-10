using System;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Core {
    /// <summary>
    /// IScope用の拡張メソッド
    /// </summary>
    public static class ScopeExtensions {
        /// <summary>
        /// ScopeのCancellationToken変換
        /// </summary>
        public static CancellationToken ToCancellationToken(this IScope source) {
            var cts = new CancellationTokenSource();
            source.OnExpired += () => { cts.Cancel(); };
            return cts.Token;
        }

        /// <summary>
        /// CancellationTokenのScope変換
        /// </summary>
        public static IScope ToScope(this CancellationToken source) {
            var scope = new DisposableScope();
            source.Register(() => { scope.Dispose(); });
            return scope;
        }

        /// <summary>
        /// IDisposableのScope登録
        /// </summary>
        public static T ScopeTo<T>(this T source, IScope scope)
            where T : IDisposable {
            scope.OnExpired += source.Dispose;
            return source;
        }

        /// <summary>
        /// GameObjectのScope登録
        /// </summary>
        public static GameObject ScopeTo(this GameObject source, IScope scope, bool immediate = false) {
            scope.OnExpired += () => {
                if (source != null) {
                    if (immediate) {
                        Object.DestroyImmediate(source);
                    }
                    else {
                        Object.Destroy(source);
                    }
                }
            };
            return source;
        }

        /// <summary>
        /// ComponentのScope登録
        /// </summary>
        public static T ScopeTo<T>(this Component source, IScope scope, bool immediate = false)
            where T : Component {
            scope.OnExpired += () => {
                if (source != null) {
                    if (immediate) {
                        Object.DestroyImmediate(source);
                    }
                    else {
                        Object.Destroy(source);
                    }
                }
            };
            return source as T;
        }
    }
}