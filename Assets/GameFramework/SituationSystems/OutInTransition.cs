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
        /// <param name="resolver">遷移処理解決者</param>
        IEnumerator ITransition.TransitRoutine(ITransitionResolver resolver) {
            resolver.Start();
            
            // 非アクティブ
            resolver.DeactivatePrev();
            
            // 閉じる
            yield return resolver.ClosePrevRoutine();
            
            // 解放
            yield return resolver.UnloadPrevRoutine();

            // 読み込み
            yield return resolver.LoadNextRoutine();
            
            // 開く
            yield return resolver.OpenNextRoutine();
            
            // アクティブ化
            resolver.ActivateNext();
            
            resolver.Finish();
        }
    }
}