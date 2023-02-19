using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;
using UnityEngine.Pool;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体オブジェクト管理クラス
    /// </summary>
    public class ProjectileObjectManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 更新モード
        /// </summary>
        public enum UpdateMode {
            Update,
            LateUpdate,
        }

        /// <summary>
        /// 飛翔オブジェクト用ハンドル
        /// </summary>
        public struct Handle : IDisposable {
            private ProjectilePlayer.Handle _projectileHandle;

            // 有効なハンドルか
            public bool IsValid => _projectileHandle.IsValid;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(ProjectilePlayer.Handle projectileHandle) {
                _projectileHandle = projectileHandle;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (!IsValid) {
                    return;
                }

                _projectileHandle.Dispose();
            }
        }

        // 更新モード
        private UpdateMode _updateMode;

        // 管理ルートになるTransform
        private Transform _rootTransform;
        // Projectile再生用Player
        private ProjectilePlayer _projectilePlayer;
        // ProjectileObjectPool管理用
        private Dictionary<GameObject, ObjectPool<ProjectileObject>> _objectPools =
            new Dictionary<GameObject, ObjectPool<ProjectileObject>>();

        // DeltaTime制御用
        public LayeredTime LayeredTime { get; } = new LayeredTime();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProjectileObjectManager(UpdateMode updateMode = UpdateMode.Update) {
            _updateMode = updateMode;
            _projectilePlayer = new ProjectilePlayer();
            
            // Rootの生成
            var rootObj = new GameObject("ProjectileObjectManager");
            UnityEngine.Object.DontDestroyOnLoad(rootObj);
            _rootTransform = rootObj.transform;
        }

        /// <summary>
        /// 飛翔オブジェクトの再生
        /// </summary>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onStopped">飛翔完了通知</param>
        public Handle Play(GameObject prefab, IProjectile projectile,
            Action<Vector3, Quaternion> onUpdatedTransform,
            Action onStopped) {
            if (prefab == null) {
                Debug.LogWarning("Projectile object prefab is null.");
                return new Handle();
            }
            
            // Poolの初期化
            if (!_objectPools.TryGetValue(prefab, out var pool)) {
                pool = new ObjectPool<ProjectileObject>(
                    () => CreateInstance(prefab), OnGetInstance, OnReleaseInstance, OnDestroyInstance);
                _objectPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var instance = pool.Get();
            instance.Setup(projectile);

            // Projectileを再生
            var projectileHandle = _projectilePlayer.Play(projectile, (pos, rot) => {
                instance.UpdateTransform(pos, rot);
                onUpdatedTransform?.Invoke(pos, rot);
            }, () => {
                instance.Cleanup();
                onStopped?.Invoke();
                pool.Release(instance);
            });

            // ハンドル化して返却
            return new Handle(projectileHandle);
        }

        /// <summary>
        /// 飛翔オブジェクトの停止
        /// </summary>
        public void Stop(Handle handle) {
            handle.Dispose();
        }

        /// <summary>
        /// 全飛翔オブジェクトの停止
        /// </summary>
        public void StopAll() {
            _projectilePlayer.StopAll();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            var deltaTime = LayeredTime.DeltaTime;
            if (_updateMode == UpdateMode.Update) {
                _projectilePlayer.Update(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = LayeredTime.DeltaTime;
            if (_updateMode == UpdateMode.LateUpdate) {
                _projectilePlayer.Update(deltaTime);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _projectilePlayer.Dispose();
            if (_rootTransform != null) {
                UnityEngine.Object.Destroy(_rootTransform.gameObject);
                _rootTransform = null;
            }
        }

        /// <summary>
        /// インスタンス生成処理
        /// </summary>
        private ProjectileObject CreateInstance(GameObject prefab) {
            var gameObj = UnityEngine.Object.Instantiate(prefab, _rootTransform);
            var instance = gameObj.GetComponent<ProjectileObject>();
            if (instance == null) {
                instance = gameObj.AddComponent<ProjectileObject>();
            }

            gameObj.SetActive(false);
            return instance;
        }

        /// <summary>
        /// インスタンス廃棄時処理
        /// </summary>
        private void OnDestroyInstance(ProjectileObject instance) {
            UnityEngine.Object.Destroy(instance.gameObject);
        }

        /// <summary>
        /// インスタンス取得時処理
        /// </summary>
        private void OnGetInstance(ProjectileObject instance) {
            instance.gameObject.SetActive(true);
        }

        /// <summary>
        /// インスタンス返却時処理
        /// </summary>
        private void OnReleaseInstance(ProjectileObject instance) {
            instance.gameObject.SetActive(false);
        }
    }
}