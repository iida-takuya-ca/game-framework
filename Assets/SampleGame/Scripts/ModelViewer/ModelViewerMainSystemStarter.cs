using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// ModelViewerを直接開始するためのStarter
    /// </summary>
    public class ModelViewerMainSystemStarter : MainSystemStarter {
        [SerializeField, Tooltip("初期化用データ")]
        private ModelViewerData _data;
        
        /// <summary>
        /// 開始シチュエーションの取得
        /// </summary>
        protected override SceneSituation GetStartSituation() {
            // Login > ModelViewerに遷移
            return new LoginSceneSituation(new ModelViewerSceneSituation(_data));
        }
    }
}