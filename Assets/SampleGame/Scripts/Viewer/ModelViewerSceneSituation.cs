using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityDebugSheet.Runtime.Core.Scripts;

namespace SampleGame {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class ModelViewerSceneSituation : SceneSituation {
        private int _debugPageId;
        
        public override string SceneAssetPath => "model_viewer";

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            _debugPageId = rootPage.AddPageLinkButton("Models", onLoad: page => {
                page.AddButton("Test1", clicked: () => {

                });
            });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal(TransitionHandle handle) {
            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            rootPage.RemoveItem(_debugPageId);
            
            base.DeactivateInternal(handle);
        }
    }
}
