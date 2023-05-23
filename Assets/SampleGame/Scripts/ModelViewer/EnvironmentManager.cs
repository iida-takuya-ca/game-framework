using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine.SceneManagement;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 環境の管理用
    /// </summary>
    public class EnvironmentManager : IDisposable {
        // Field生成中のScope
        private DisposableScope _fieldScope = new();
        
        // 現在使用中のFieldScene
        public Scene CurrentFieldScene { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            RemoveEnvironment();
        }
        
        /// <summary>
        /// 環境の変更
        /// </summary>
        public async UniTask ChangeEnvironmentAsync(string assetId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            RemoveEnvironment();
            
            CurrentFieldScene = await new FieldSceneAssetRequest(assetId)
                .LoadAsync(true, _fieldScope, ct);
        }

        /// <summary>
        /// 環境の削除
        /// </summary>
        public void RemoveEnvironment() {
            if (!CurrentFieldScene.IsValid()) {
                return;
            }

            SceneManager.UnloadSceneAsync(CurrentFieldScene);
            _fieldScope.Clear();
            CurrentFieldScene = new Scene();
        }
    }
}
