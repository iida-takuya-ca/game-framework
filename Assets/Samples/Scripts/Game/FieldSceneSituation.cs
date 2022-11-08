using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// Field用のSituation
    /// </summary>
    public class FieldSceneSituation : SceneSituation {
        // FieldScene内シチュエーション用コンテナ
        private SituationContainer _situationContainer;
        
        public override string SceneAssetPath => "field";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.Space)) {
                ParentContainer.Transition(new BattleSceneSituation());
            }
        }
    }
}
