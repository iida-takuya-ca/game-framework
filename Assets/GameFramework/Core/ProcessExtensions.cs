using System.Threading;
#if USE_UNI_TASK
using Cysharp.Threading.Tasks;
#endif

namespace GameFramework.Core {
    /// <summary>
    /// IProcess用の拡張メソッド
    /// </summary>
    public static class ProcessExtensions {
#if USE_UNI_TASK
        /// <summary>
        /// IProcessをUniTaskに変換
        /// </summary>
        public static UniTask ToUniTask(this IProcess source, CancellationToken cancellationToken) {
            return UniTask.WaitUntil(() => source.IsDone, PlayerLoopTiming.Update, cancellationToken);
        }
#endif
    }
}