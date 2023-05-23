using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityDebugSheet.Runtime.Core.Scripts;
using SampleGame.ModelViewer;
using UnityEngine.InputSystem.Utilities;
using Observable = UniRx.Observable;

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
            
            // フィールドの読み込み
            yield return environmentManager.ChangeEnvironmentAsync(_modelViewerData.defaultEnvironmentId, ct);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var ct = scope.ToCancellationToken();
            var viewerModel = ModelViewerModel.Get();
            var appService = Services.Get<ModelViewerApplicationService>();
            var entityManager = Services.Get<EntityManager>();
            var environmentManager = Services.Get<EnvironmentManager>();
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
                            motionsPageTuple.page.AddButton(clip.name, clicked: () => {
                                // Actor取得
                                var actor = entityManager.PreviewActorProperty.Value;
                                actor.ChangeMotion(clip);
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
                            clicked: () => environmentManager.ChangeEnvironmentAsync(id, ct).Forget());
                    }
                });
                
                // PreviewObject
                pageTuple.page.AddPageLinkButton("Models", onLoad: modelsPageTuple => {
                    
                    var bodyDataIds = viewerModel.Data.bodyDataIds;
                    foreach (var bodyDataId in bodyDataIds) {
                        var id = bodyDataId;
                        modelsPageTuple.page.AddButton(bodyDataId, clicked:() => entityManager.CreatePreviewObjectAsync(id, ct).Forget());
                    }
                });
                    
                // 初期状態反映
                environmentManager.ChangeEnvironmentAsync(_modelViewerData.defaultEnvironmentId, ct).Forget();
                entityManager.CreatePreviewObjectAsync(_modelViewerData.defaultBodyDataId, ct).Forget();
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
