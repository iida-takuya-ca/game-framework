using System;
using System.Collections.Generic;
using GameFramework.CollisionSystems;
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

        /// <summary>
        /// 再生中情報
        /// </summary>
        private class PlayingInfo {
            public ObjectPool<IProjectileObject> pool;
            public IProjectileObject projectileObject;
            public ProjectilePlayer.Handle projectileHandle;
        }

        // コリジョンマネージャ
        private CollisionManager _collisionManager;
        // 更新モード
        private UpdateMode _updateMode;

        // 管理ルートになるTransform
        private Transform _rootTransform;
        // Projectile再生用Player
        private ProjectilePlayer _projectilePlayer;
        // ProjectileObjectPool管理用
        private Dictionary<GameObject, ObjectPool<IProjectileObject>> _objectPools =
            new Dictionary<GameObject, ObjectPool<IProjectileObject>>();
        // 再生中情報
        private List<PlayingInfo> _playingInfos = new List<PlayingInfo>();

        // DeltaTime制御用
        public LayeredTime LayeredTime { get; } = new LayeredTime();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProjectileObjectManager(CollisionManager collisionManager, UpdateMode updateMode = UpdateMode.Update) {
            _collisionManager = collisionManager;
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
        /// <param name="listener">当たり判定通知用リスナー</param>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="hitLayerMask">当たり判定に使うLayerMask</param>
        /// <param name="hitCount">最大衝突回数(-1だと無限)</param>
        /// <param name="customData">当たり判定通知時に使うカスタムデータ</param>
        /// <param name="checkHitFunc">当たりとして有効かの判定関数</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onStopped">飛翔完了通知</param>
        public Handle Play(IRaycastCollisionListener listener, GameObject prefab, IProjectile projectile,
            int hitLayerMask, int hitCount = -1, object customData = null, Func<RaycastHitResult, bool> checkHitFunc = null,
            Action<Vector3, Quaternion> onUpdatedTransform = null,
            Action onStopped = null) {
            if (prefab == null) {
                Debug.LogWarning("Projectile object prefab is null.");
                return new Handle();
            }

            // Poolの初期化
            if (!_objectPools.TryGetValue(prefab, out var pool)) {
                pool = new ObjectPool<IProjectileObject>(
                    () => CreateInstance(prefab), OnGetInstance, OnReleaseInstance, OnDestroyInstance);
                _objectPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var instance = pool.Get();
            instance.StartProjectile(projectile);

            // Raycastの生成
            var raycastCollision = (IRaycastCollision)(instance.RaycastRadius > float.Epsilon
                ? new SphereRaycastCollision(projectile.Position, projectile.Position, instance.RaycastRadius)
                : new LineRaycastCollision(projectile.Position, projectile.Position));
            var collisionHandle = new CollisionHandle();

            // Projectileを再生
            var projectileHandle = _projectilePlayer.Play(projectile, (pos, rot) => {
                instance.UpdateTransform(pos, rot);
                raycastCollision.March(pos);
                onUpdatedTransform?.Invoke(pos, rot);
            }, () => {
                collisionHandle.Dispose();
                instance.ExitProjectile();
                onStopped?.Invoke();
            });

            // コリジョン登録
            collisionHandle = _collisionManager.Register(raycastCollision, hitLayerMask, customData, result => {
                if (checkHitFunc != null && !checkHitFunc.Invoke(result)) {
                    return;
                }
                instance.OnHitCollision(result);
                listener.OnHitRaycastCollision(result);
                if (hitCount >= 0 && result.hitCount >= hitCount) {
                    projectileHandle.Dispose();
                }
            });

            // 再生情報として登録
            _playingInfos.Add(new PlayingInfo {
                pool = pool,
                projectileObject = instance,
                projectileHandle = projectileHandle
            });

            // ハンドル化して返却
            return new Handle(projectileHandle);
        }

        /// <summary>
        /// 飛翔オブジェクトの再生
        /// </summary>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="hitLayerMask">当たり判定に使うLayerMask</param>
        /// <param name="hitCount">最大衝突回数(-1だと無限)</param>
        /// <param name="customData">当たり判定通知時に使うカスタムデータ</param>
        /// <param name="checkHitFunc">当たりとして有効かの判定関数</param>
        /// <param name="onHitRaycastCollision">当たり判定発生時通知</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onStopped">飛翔完了通知</param>
        public Handle Play(GameObject prefab, IProjectile projectile,
            int hitLayerMask, int hitCount = -1, object customData = null, Func<RaycastHitResult, bool> checkHitFunc = null,
            Action<RaycastHitResult> onHitRaycastCollision = null,
            Action<Vector3, Quaternion> onUpdatedTransform = null,
            Action onStopped = null) {
            var listener = new RaycastCollisionListener();
            listener.OnHitRaycastCollisionEvent += onHitRaycastCollision;
            return Play(listener, prefab, projectile, hitLayerMask, hitCount, customData, checkHitFunc,
                onUpdatedTransform, onStopped);
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
        /// </summary>tile
        protected override void UpdateInternal() {
            if (_updateMode == UpdateMode.Update) {
                var deltaTime = LayeredTime.DeltaTime;
                UpdateProjectileObjects(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            if (_updateMode == UpdateMode.LateUpdate) {
                var deltaTime = LayeredTime.DeltaTime;
                UpdateProjectileObjects(deltaTime);
            }
        }

        private void UpdateProjectileObjects(float deltaTime) {
            _projectilePlayer.Update(deltaTime);

            // ProjectileObjectの更新
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];
                info.projectileObject.UpdateProjectile(deltaTime);

                // 再生完了していたらPoolに返却する
                if (!info.projectileObject.IsPlaying) {
                    info.pool.Release(info.projectileObject);
                    _playingInfos.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _projectilePlayer.Dispose();

            foreach (var info in _playingInfos) {
                info.pool.Release(info.projectileObject);
            }

            foreach (var pool in _objectPools.Values) {
                pool.Dispose();
            }

            _objectPools.Clear();

            if (_rootTransform != null) {
                UnityEngine.Object.Destroy(_rootTransform.gameObject);
                _rootTransform = null;
            }
        }

        /// <summary>
        /// インスタンス生成処理
        /// </summary>
        private IProjectileObject CreateInstance(GameObject prefab) {
            var gameObj = UnityEngine.Object.Instantiate(prefab, _rootTransform);
            var instance = gameObj.GetComponent<IProjectileObject>();
            if (instance == null) {
                instance = gameObj.AddComponent<ProjectileObject>();
            }

            instance.SetActive(false);
            return instance;
        }

        /// <summary>
        /// インスタンス廃棄時処理
        /// </summary>
        private void OnDestroyInstance(IProjectileObject instance) {
            instance.Dispose();
        }

        /// <summary>
        /// インスタンス取得時処理
        /// </summary>
        private void OnGetInstance(IProjectileObject instance) {
            instance.SetActive(true);
        }

        /// <summary>
        /// インスタンス返却時処理
        /// </summary>
        private void OnReleaseInstance(IProjectileObject instance) {
            instance.SetActive(false);
        }
    }
}