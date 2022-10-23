using System;
using System.Collections;
using GameFramework.Core;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// ゲーム用の定義処理
    /// </summary>
    public static class Extensions {
        /// <summary>
        /// IObservableのコルーチン変換
        /// </summary>
        public static IEnumerator StartAsEnumerator<T>(this IObservable<T> source, IScope scope) {
            var finished = false;
            source
                .Subscribe(_ => {}, () => finished = true)
                .ScopeTo(scope);
            while (!finished) {
                yield return null;
            }
        }
    }
}