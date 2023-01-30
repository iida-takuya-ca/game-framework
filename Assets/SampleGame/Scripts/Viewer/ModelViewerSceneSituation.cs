using System;
using System.Collections;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityDebugSheet.Runtime.Core.Scripts;

namespace SampleGame {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class ModelViewerSceneSituation : SceneSituation {
        private int _debugPageId;
        
        protected override string SceneAssetPath => "model_viewer";

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            var bodyManager = new BodyManager();
            bodyManager.RegisterTask(TaskOrder.Body);
            ServiceContainer.Set(bodyManager);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var viewerManager = Services.Get<ModelViewerManager>();
            var debugSheet = Services.Get<DebugSheet>();
            
            var rootPage = debugSheet.GetOrCreateInitialPage();
            var motionsPageId = -1;
            _debugPageId = rootPage.AddPageLinkButton("Model Viewer", onLoad: page => {
                page.AddPageLinkButton("Models", onLoad: modelsPage => {
                    var bodyDataIds = viewerManager.BodyDataIds;
                    foreach (var bodyDataId in bodyDataIds) {
                        var id = bodyDataId;
                        modelsPage.AddButton(bodyDataId, clicked:() =>
                        {
                            viewerManager.CleanupBody();
                            
                            if (motionsPageId >= 0) {
                                page.RemoveItem(motionsPageId);
                            }

                            viewerManager.SetupBody(id, () => {
                                motionsPageId = page.AddPageLinkButton("Motions", onLoad: motionsPage => {
                                    var clips = viewerManager.CurrentBodyData.animationClips;
                                    for (var i = 0; i < clips.Length; i++) {
                                        var clip = clips[i];
                                        var index = i;
                                        motionsPage.AddButton(clip.name, clicked: () => {
                                            viewerManager.SetMotion(index);
                                        });
                                    }
                                });
                            });
                        });
                    }
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
