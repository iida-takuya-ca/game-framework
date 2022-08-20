using System.Collections;
using System.Collections.Generic;
using GameFramework.CoroutineSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 閉じると開くを同時に行う遷移
    /// </summary>
    public class CrossTransition : ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="transitionInfo">遷移情報</param>
        IEnumerator ITransition.TransitRoutine(Situation.TransitionInfo transitionInfo) {
            var handle = new TransitionHandle(transitionInfo);
            var prev = transitionInfo.prev;
            var next = transitionInfo.next;

            transitionInfo.state = TransitionState.Standby;

            if (next != null) {
                // 初期化開始
                transitionInfo.state = TransitionState.Initializing;

                // 読み込み処理
                yield return next.LoadRoutine(handle);
            
                // 初期化処理
                next.Setup(handle);
            }

            // オープン開始
            transitionInfo.state = TransitionState.Opening;
            var functions = new List<IEnumerator>();
            if (prev != null) {
                functions.Add(prev.CloseRoutine(handle));
            }
            if (next != null) {
                functions.Add(next.OpenRoutine(handle));
            }
            yield return new MergedCoroutine(functions.ToArray());

            if (prev != null) {
                // 終了処理
                prev.Cleanup(handle);
                
                // 解放処理
                prev.Unload(handle);
            }
            
            // 完了
            transitionInfo.state = TransitionState.Completed;
        }
    }
}