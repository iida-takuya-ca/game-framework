using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using UnityEngine;

#if USE_UNI_TASK
using Cysharp.Threading.Tasks;
#endif

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// コルーチンの拡張
    /// </summary>
    public static class CoroutineExtensions {
        // WaitForSeconds.m_Seconds
        private static readonly FieldInfo WaitForSecondsAccessor = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

        /// <summary>
        /// WaitForSecondsのIEnumerator変換
        /// </summary>
        public static IEnumerator GetEnumerator(this WaitForSeconds source) {
            var second = (float)WaitForSecondsAccessor.GetValue(source);
            var startTime = DateTimeOffset.UtcNow;
            while (true) {
                yield return null;
                var elapsed = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
                if (elapsed >= second) {
                    break;
                }
            }
        }

#if USE_UNI_TASK
        /// <summary>
        /// コルーチン実行（UniTask）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="enumerator"></param>
        /// <param name="cancellationToken"></param>
        public static UniTask StartCoroutineAsync(this CoroutineRunner source, IEnumerator enumerator, CancellationToken cancellationToken) {
            var completionSource = new UniTaskCompletionSource();
            if (cancellationToken.IsCancellationRequested) {
                completionSource.TrySetCanceled(cancellationToken);
                return completionSource.Task;
            }
            
            source.StartCoroutine(enumerator, () => {
                completionSource.TrySetResult();
            }, () => {
                completionSource.TrySetCanceled();
            }, exception => {
                completionSource.TrySetException(exception);
            }, cancellationToken);
            
            return completionSource.Task;
        }
#endif
    }
}
