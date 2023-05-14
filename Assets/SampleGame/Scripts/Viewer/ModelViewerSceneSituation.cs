using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityDebugSheet.Runtime.Core.Scripts;
using SampleGame.Viewer;

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

            var ct = scope.ToCancellationToken();
            
            var bodyManager = new BodyManager();
            bodyManager.RegisterTask(TaskOrder.Body);
            ServiceContainer.Set(bodyManager);
            
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
            cameraManager.SetCameraController("Default", new ModelViewerCameraController());
            
            var viewerManager = Services.Get<ModelViewerManager>();
            viewerManager.RegisterTask(TaskOrder.Logic);
            yield return viewerManager.InitializeAsync(ct)
                .ToCoroutine();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var ct = scope.ToCancellationToken();
            var viewerManager = Services.Get<ModelViewerManager>();
            var debugSheet = Services.Get<DebugSheet>();
            
            var rootPage = debugSheet.GetOrCreateInitialPage();
            var motionsPageId = -1;
            _debugPageId = rootPage.AddPageLinkButton("Model Viewer", onLoad: pageTuple => {
                // モーションページの初期化
                void SetupMotionPage(ModelViewerBodyData bodyData) {
                    if (motionsPageId >= 0) {
                        pageTuple.page.RemoveItem(motionsPageId);
                        motionsPageId = -1;
                    }

                    if (bodyData == null) {
                        return;
                    }
                        
                    motionsPageId = pageTuple.page.AddPageLinkButton("Motions", onLoad: motionsPageTuple => {
                        var clips = bodyData.animationClips;
                        for (var i = 0; i < clips.Length; i++) {
                            var clip = clips[i];
                            var index = i;
                            motionsPageTuple.page.AddButton(clip.name, clicked: () => {
                                viewerManager.SetMotion(index);
                            });
                        }
                    });
                }
                
                // Field
                pageTuple.page.AddPageLinkButton("Fields", onLoad: fieldsPageTuple => {
                    
                    var fieldIds = viewerManager.FieldIds;
                    foreach (var fieldId in fieldIds) {
                        var id = fieldId;
                        fieldsPageTuple.page.AddButton(fieldId, clicked:() =>
                        {
                            viewerManager.SetupFieldAsync(id, ct)
                                .Forget();
                        });
                    }
                });
                
                // Model
                pageTuple.page.AddPageLinkButton("Models", onLoad: modelsPageTuple => {
                    
                    var bodyDataIds = viewerManager.BodyDataIds;
                    foreach (var bodyDataId in bodyDataIds) {
                        var id = bodyDataId;
                        modelsPageTuple.page.AddButton(bodyDataId, clicked:() =>
                        {
                            viewerManager.SetupBodyAsync(id, ct)
                                .ContinueWith(() => SetupMotionPage(viewerManager.CurrentBodyData));
                        });
                    }
                });
                    
                // 初期状態反映
                SetupMotionPage(viewerManager.CurrentBodyData);
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
