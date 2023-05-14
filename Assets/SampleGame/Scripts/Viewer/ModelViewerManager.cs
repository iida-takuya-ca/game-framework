using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.PlayableSystems;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.Viewer {
    /// <summary>
    /// モデルビューア管理用クラス
    /// </summary>
    public class ModelViewerManager : LateUpdatableTaskBehaviour, IDisposable {
        [SerializeField, Tooltip("ビューア用のデータ")]
        private ModelViewerData _data;

        // Field生成中のScope
        private DisposableScope _fieldScope = new();
        // アセット格納用ストレージ
        private PoolAssetStorage<ModelViewerBodyData> _bodyDataPoolAssetStorage;

        // 読み込み可能なFieldのIdリスト
        public string[] FieldIds => _data.fieldIds;
        // 読み込み可能なBodyDataのIdリスト
        public string[] BodyDataIds => _data.bodyDataIds;
        // 現在使用中のBodyData
        public ModelViewerBodyData CurrentBodyData { get; private set; }
        // 現在使用中のBody
        public Body CurrentBody { get; private set; }
        // 現在使用中のFieldScene
        public Scene CurrentFieldScene { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken ct) {
            var assetManager = Services.Get<AssetManager>();
            _bodyDataPoolAssetStorage = new PoolAssetStorage<ModelViewerBodyData>(assetManager, 2);
            
            // 初期のフィールドを読み込み
            await SetupFieldAsync(_data.defaultFieldId, ct);
            // 初期のBodyDataを読み込み
            await SetupBodyAsync(_data.defaultBodyDataId, ct);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_bodyDataPoolAssetStorage != null) {
                _bodyDataPoolAssetStorage.Dispose();
                _bodyDataPoolAssetStorage = null;
            }
        }

        /// <summary>
        /// フィールドの初期化
        /// </summary>
        public async UniTask SetupFieldAsync(string fieldId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            CleanupField();
            
            CurrentFieldScene = await new FieldSceneAssetRequest(fieldId)
                .LoadAsync(true, _fieldScope, ct);
        }

        /// <summary>
        /// 現在生成されているBodyの削除
        /// </summary>
        public void CleanupField() {
            if (!CurrentFieldScene.IsValid()) {
                return;
            }

            SceneManager.UnloadSceneAsync(CurrentFieldScene);
            _fieldScope.Clear();
            CurrentFieldScene = new Scene();
        }

        /// <summary>
        /// Bodyの生成
        /// </summary>
        public async UniTask SetupBodyAsync(string bodyDataId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            CleanupBody();

            var data = await _bodyDataPoolAssetStorage.LoadAssetAsync(new ModelViewerBodyDataRequest(bodyDataId))
                .ToUniTask(cancellationToken:ct);

            if (data == null) {
                return;
            }
            
            // Bodyの生成
            var bodyManager = Services.Get<BodyManager>();
            CurrentBodyData = data;
            CurrentBody = bodyManager.CreateFromPrefab(data.prefab);
            
            // ApplyRootMotionを切る
            var animator = CurrentBody.GetComponent<Animator>();
            if (animator != null) {
                animator.applyRootMotion = false;
            }
            
            // 位置初期化
            CurrentBody.Position = Vector3.zero;
            CurrentBody.Rotation = Quaternion.identity;
            CurrentBody.BaseScale = 1.0f;
            
            // モーション適用
            SetMotion(0);
        }

        /// <summary>
        /// 現在生成されているBodyの削除
        /// </summary>
        public void CleanupBody() {
            if (CurrentBodyData == null) {
                return;
            }

            if (CurrentBody != null) {
                CurrentBody.Dispose();
                CurrentBody = null;
            }

            CurrentBodyData = null;
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(int index) {
            if (CurrentBody == null) {
                return;
            }

            if (index < 0 || index >= CurrentBodyData.animationClips.Length) {
                return;
            }

            var clip = CurrentBodyData.animationClips[index];
            if (clip == null) {
                return;
            }

            var motionController = CurrentBody.GetController<MotionController>();
            if (motionController == null) {
                return;
            }

            motionController.Player.Change(clip, 0.2f);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void OnDestroyInternal() {
            Dispose();
            base.OnDestroyInternal();
        }
    }
}
