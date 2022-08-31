using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

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
    }
}
