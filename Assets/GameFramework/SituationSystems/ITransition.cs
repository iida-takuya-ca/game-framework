using System.Collections;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 遷移処理用インターフェース
    /// </summary>
    public interface ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="transitionInfo">遷移情報</param>
        IEnumerator TransitRoutine(Situation.TransitionInfo transitionInfo);
    }
}