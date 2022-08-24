using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.CoroutineSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 閉じてから開く遷移処理
    /// </summary>
    public class OutInTransition : ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="transitionInfo">遷移情報</param>
        IEnumerator ITransition.TransitRoutine(SituationContainer.TransitionInfo transitionInfo) {
            var handle = new TransitionHandle(transitionInfo);
            var prev = transitionInfo.prev;
            var next = transitionInfo.next;

            transitionInfo.state = TransitionState.Standby;

            if (prev != null) {
                // 演出開始 & 閉じる処理
                var functions = new List<IEnumerator>();
                functions.Add(prev.CloseRoutine(handle));
                functions.AddRange(transitionInfo.effects.Select(x => x.EnterRoutine()));
                yield return new MergedCoroutine(functions.ToArray());
                
                // 終了処理
                prev.Cleanup(handle);
                
                // 解放処理
                prev.Unload(handle);
            }
            
            // 派生用初期化直前処理
            yield return PreInitializeRoutine(transitionInfo);

            if (next != null) {
                // 初期化開始
                transitionInfo.state = TransitionState.Initializing;

                // 演出更新 & 読み込み処理
                yield return new AdditionalProcessCoroutine(next.LoadRoutine(handle), () => {
                    foreach (var effect in transitionInfo.effects) {
                        effect.Update();
                    }
                });
            
                // 初期化処理
                next.Setup(handle);

                // オープン開始
                transitionInfo.state = TransitionState.Opening;
                
                // 演出終了 & 開く処理
                var functions = new List<IEnumerator>();
                functions.Add(next.OpenRoutine(handle));
                functions.AddRange(transitionInfo.effects.Select(x => x.ExitRoutine()));
                yield return new MergedCoroutine(functions.ToArray());
            }
            
            // 完了
            transitionInfo.state = TransitionState.Completed;
        }

        /// <summary>
        /// 初期化前処理
        /// </summary>
        protected virtual IEnumerator PreInitializeRoutine(SituationContainer.TransitionInfo transitionInfo) { yield break; }
    }
}