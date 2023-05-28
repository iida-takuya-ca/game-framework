using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityDebugSheet.Runtime.Core.Scripts;
using SampleGame.ModelViewer;

namespace SampleGame {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class ModelViewerSceneSituation : SceneSituation {
        private int _debugPageId;
        private ModelViewerData _modelViewerData;
        
        protected override string SceneAssetPath => "model_viewer";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerSceneSituation(ModelViewerData modelViewerData) {
            _modelViewerData = modelViewerData;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            var ct = scope.ToCancellationToken();
            
            // Modelの生成
            var viewerModel = ModelViewerModel.Create()
                .ScopeTo(scope);
            viewerModel.Setup(_modelViewerData);
            
            // Repositoryの生成
            var repository = new ModelViewerRepository(Services.Get<AssetManager>());
            repository.ScopeTo(scope);
            
            // ApplicationServiceの生成
            var appService = new ModelViewerApplicationService(viewerModel, repository);
            ServiceContainer.Set(appService);
            
            // 各種管理クラス生成/初期化
            var bodyManager = new BodyManager();
            bodyManager.RegisterTask(TaskOrder.Body);
            ServiceContainer.Set(bodyManager);

            var environmentManager = new EnvironmentManager();
            ServiceContainer.Set(environmentManager);

            var entityManager = new EntityManager();
            ServiceContainer.Set(entityManager);
            
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
            
            // カメラ操作用Controllerの設定
            cameraManager.SetCameraController("Default", new PreviewCameraController());
                    
            // 初期状態反映
            appService.ChangeEnvironment(_modelViewerData.defaultEnvironmentId);
            appService.ChangePreviewActorAsync(_modelViewerData.defaultActorDataId, ct).Forget();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var ct = scope.ToCancellationToken();
            var viewerModel = ModelViewerModel.Get();
            var appService = Services.Get<ModelViewerApplicationService>();
            
            // Presenter初期化
            var presenter = new ModelViewerPresenter(viewerModel)
                .ScopeTo(scope);
            presenter.RegisterTask(TaskOrder.Logic);
            presenter.Activate();
            
            // DebugPage初期化
            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            var motionsPageId = -1;
            _debugPageId = rootPage.AddPageLinkButton("Model Viewer", onLoad: pageTuple => {
                // モーションページの初期化
                void SetupMotionPage(PreviewActorSetupData setupData) {
                    if (motionsPageId >= 0) {
                        pageTuple.page.RemoveItem(motionsPageId);
                        motionsPageId = -1;
                    }

                    if (setupData == null) {
                        return;
                    }
                        
                    motionsPageId = pageTuple.page.AddPageLinkButton("Motions", onLoad: motionsPageTuple => {
                        var clips = setupData.animationClips;
                        for (var i = 0; i < clips.Length; i++) {
                            var index = i;
                            var clip = clips[i];
                            motionsPageTuple.page.AddButton(clip.name, clicked: () => {
                                viewerModel.PreviewActor.ChangeClip(index);
                            });
                        }
                    });
                }
                
                // Environment
                pageTuple.page.AddPageLinkButton("Environments", onLoad: fieldsPageTuple => {
                    var environmentIds = viewerModel.Data.environmentIds;
                    foreach (var environmentId in environmentIds) {
                        var id = environmentId;
                        fieldsPageTuple.page.AddButton(environmentId,
                            clicked: () => appService.ChangeEnvironment(id));
                    }
                });
                
                // PreviewActor
                pageTuple.page.AddPageLinkButton("Models", onLoad: modelsPageTuple => {
                    var actorDataIds = viewerModel.Data.actorDataIds;
                    foreach (var actorDataId in actorDataIds) {
                        var id = actorDataId;
                        modelsPageTuple.page.AddButton(actorDataId, clicked:() => {
                            appService.ChangePreviewActorAsync(id, ct)
                                .ContinueWith(SetupMotionPage);
                        });
                    }
                });
                
                // 初期状態反映
                SetupMotionPage(viewerModel.PreviewActor.SetupData.Value);
            });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal(TransitionHandle handle) {
            // Debugページ削除
            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            rootPage.RemoveItem(_debugPageId);
            
            base.DeactivateInternal(handle);
        }
    }
}
