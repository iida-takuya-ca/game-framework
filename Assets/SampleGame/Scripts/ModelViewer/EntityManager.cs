using System;
using Cysharp.Threading.Tasks;
using GameFramework.EntitySystems;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// Entity管理用クラス
    /// </summary>
    public class EntityManager : IDisposable {
        private Entity _previewObjectEntity;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
        }

        public async UniTask<Entity> CreatePreviewObjectAsync() {
            
        }
    }
}
