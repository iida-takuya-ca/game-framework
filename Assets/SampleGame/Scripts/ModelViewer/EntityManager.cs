using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using UniRx;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// Entity管理用クラス
    /// </summary>
    public class EntityManager : IDisposable {
        private Entity _previewObjectEntity;
        private ReactiveProperty<PreviewObjectActor> _previewActorProperty = new();

        // 現在のプレビュー用アクター
        public IReadOnlyReactiveProperty<PreviewObjectActor> PreviewActorProperty => _previewActorProperty;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_previewObjectEntity != null) {
                _previewActorProperty.Value = null;
                _previewObjectEntity.Dispose();
                _previewObjectEntity = null;
            }
        }

        /// <summary>
        /// PreviewObjectの生成
        /// </summary>
        public async UniTask<Entity> CreatePreviewObjectAsync(string bodyDataId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            if (_previewObjectEntity == null) {
                _previewObjectEntity = new Entity();
            }
            
            // BodyData読み込み
            var bodyData = await new ModelViewerBodyDataRequest(bodyDataId)
                .LoadAsync(Services.Get<AssetManager>())
                .ToUniTask(cancellationToken: ct);

            // 既存のActor/Bodyを削除
            _previewActorProperty.Value = null;
            _previewObjectEntity.RemoveActors();
            _previewObjectEntity.RemoveBody();

            if (bodyData == null) {
                return null;
            }
            
            // Bodyの生成
            var bodyManager = Services.Get<BodyManager>();
            var body = bodyManager.CreateFromPrefab(bodyData.prefab);
            _previewObjectEntity.SetBody(body);
            
            // Actorの生成
            var actor = new PreviewObjectActor(body);
            actor.RegisterTask(TaskOrder.Actor);
            _previewObjectEntity.AddActor(actor);

            // アクターの置き換え
            _previewActorProperty.Value = actor;

            return _previewObjectEntity;
        }
    }
}
