using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューア用のリポジトリ
    /// </summary>
    public class ModelViewerRepository : IDisposable {
        private readonly AssetManager _assetManager;
        private readonly DisposableScope _unloadScope;
        private PoolAssetStorage<PreviewActorSetupData> _actorSetupDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerRepository(AssetManager assetManager) {
            _assetManager = assetManager;
            _unloadScope = new DisposableScope();
            _actorSetupDataStorage = new PoolAssetStorage<PreviewActorSetupData>(_assetManager, 2);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _actorSetupDataStorage.Dispose();
            _unloadScope.Dispose();
        }

        /// <summary>
        /// ActorSetupDataの読み込み
        /// </summary>
        public UniTask<ModelViewerSetupData> LoadSetupDataAsync(CancellationToken ct) {
            return new ModelViewerSetupDataRequest()
                .LoadAsync(_assetManager, _unloadScope)
                .ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// ActorDataの読み込み
        /// </summary>
        public UniTask<PreviewActorSetupData> LoadActorSetupDataAsync(string setupDataId, CancellationToken ct) {
            return _actorSetupDataStorage.LoadAssetAsync(new PreviewActorSetupDataRequest(setupDataId))
                .ToUniTask(cancellationToken:ct);
        }
    }
}
