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
        private AssetManager _assetManager;
        private PoolAssetStorage<ModelViewerBodyData> _bodyDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerRepository(AssetManager assetManager) {
            _assetManager = assetManager;
            _bodyDataStorage = new PoolAssetStorage<ModelViewerBodyData>(_assetManager, 2);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _bodyDataStorage.Dispose();
        }

        /// <summary>
        /// BodyDataの読み込み
        /// </summary>
        public UniTask<ModelViewerBodyData> LoadBodyDataAsync(string bodyDataId, CancellationToken ct) {
            return _bodyDataStorage.LoadAssetAsync(new ModelViewerBodyDataRequest(bodyDataId))
                .ToUniTask(cancellationToken:ct);
        }
    }
}
