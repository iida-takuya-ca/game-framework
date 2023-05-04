using System;
using System.Collections.Generic;
using GameFramework.TaskSystems;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョン管理クラス
    /// </summary>
    public class CollisionManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 更新モード
        /// </summary>
        public enum UpdateMode {
            Update,
            LateUpdate,
        }

        /// <summary>
        /// コリジョン情報
        /// </summary>
        private class CollisionInfo {
            public bool destroy;
            public ICollisionListener listener;
            public ICollision collision;
            public int layerMask;
            public object customData;
        }

        /// <summary>
        /// レイキャストコリジョン情報
        /// </summary>
        private class RaycastCollisionInfo {
            public bool destroy;
            public IRaycastCollisionListener listener;
            public IRaycastCollision collision;
            public int layerMask;
            public int hitCount;
            public object customData;
        }

        // 更新モード
        private UpdateMode _updateMode;

        // 登録済みコリジョン情報
        private List<CollisionInfo> _collisionInfos = new List<CollisionInfo>();
        private List<RaycastCollisionInfo> _raycastCollisionInfos = new List<RaycastCollisionInfo>();

        // 結果格納用ワーク
        private List<Collider> _workResults = new List<Collider>();
        private List<RaycastHit> _workRaycastResults = new List<RaycastHit>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="updateMode">更新モード</param>
        public CollisionManager(UpdateMode updateMode = UpdateMode.LateUpdate) {
            _updateMode = updateMode;
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="listener">通知を受け取るためのリスナー</param>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(ICollisionListener listener, ICollision collision, int layerMask,
            object customData, bool clearHistory = true) {
            var collisionInfo = new CollisionInfo {
                listener = listener,
                collision = collision,
                layerMask = layerMask,
                customData = customData
            };

            if (clearHistory) {
                collision.ClearHistory();
            }

            // 管理リストに登録
            _collisionInfos.Add(collisionInfo);

            // ハンドル生成
            var handle = new CollisionHandle(this, collisionInfo);

            // Debug用の登録
            CollisionVisualizer.Register(collision);

            return handle;
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="onHitCollision">当たり判定通知用のCallback</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(ICollision collision, int layerMask, object customData,
            Action<HitResult> onHitCollision,
            bool clearHistory = true) {
            var listener = new CollisionListener();
            listener.OnHitCollisionEvent += onHitCollision;
            return Register(listener, collision, layerMask, customData, clearHistory);
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="listener">通知を受け取るためのリスナー</param>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(IRaycastCollisionListener listener, IRaycastCollision collision, int layerMask,
            object customData, bool clearHistory = true) {
            var collisionInfo = new RaycastCollisionInfo {
                listener = listener,
                collision = collision,
                layerMask = layerMask,
                customData = customData
            };

            if (clearHistory) {
                collision.ClearHistory();
            }

            // 管理リストに登録
            _raycastCollisionInfos.Add(collisionInfo);

            // ハンドル生成
            var handle = new CollisionHandle(this, collisionInfo);

            // Debug用の登録
            CollisionVisualizer.Register(collision);

            return handle;
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="onHitRaycastCollision">当たり判定通知用のCallback</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(IRaycastCollision collision, int layerMask, object customData,
            Action<RaycastHitResult> onHitRaycastCollision,
            bool clearHistory = true) {
            var listener = new RaycastCollisionListener();
            listener.OnHitRaycastCollisionEvent += onHitRaycastCollision;
            return Register(listener, collision, layerMask, customData, clearHistory);
        }

        /// <summary>
        /// コリジョンの登録解除
        /// </summary>
        /// <param name="handle">登録時に入手したハンドル</param>
        public void Unregister(CollisionHandle handle) {
            if (!handle.IsValid) {
                return;
            }

            // 登録されていたら削除フラグを立てる
            if (handle.Key is CollisionInfo collisionInfo && !collisionInfo.destroy) {
                collisionInfo.destroy = true;
            }

            if (handle.Key is RaycastCollisionInfo raycastCollisionInfo && !raycastCollisionInfo.destroy) {
                raycastCollisionInfo.destroy = true;
            }

            // ハンドルを無効化
            handle.Dispose();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            // 削除済みの物をクリア
            for (var i = 0; i < _collisionInfos.Count; i++) {
                // Debug用の登録解除
                CollisionVisualizer.Unregister(_collisionInfos[i].collision);
            }

            for (var i = 0; i < _raycastCollisionInfos.Count; i++) {
                // Debug用の登録解除
                CollisionVisualizer.Unregister(_raycastCollisionInfos[i].collision);
            }

            _collisionInfos.Clear();
            _raycastCollisionInfos.Clear();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            if (_updateMode == UpdateMode.Update) {
                UpdateCollisionInfos();
                UpdateRaycastCollisionInfos();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            if (_updateMode == UpdateMode.LateUpdate) {
                UpdateCollisionInfos();
                UpdateRaycastCollisionInfos();
            }
        }

        /// <summary>
        /// コリジョン情報の更新
        /// </summary>
        private void UpdateCollisionInfos() {
            // 削除済みの物をクリア
            for (var i = _collisionInfos.Count - 1; i >= 0; i--) {
                if (!_collisionInfos[i].destroy) {
                    continue;
                }

                // Debug用の登録解除
                CollisionVisualizer.Unregister(_collisionInfos[i].collision);

                _collisionInfos.RemoveAt(i);
            }

            // 当たり判定実行
            var hitResult = new HitResult();
            for (var i = 0; i < _collisionInfos.Count; i++) {
                var info = _collisionInfos[i];
                _workResults.Clear();

                if (!info.collision.Tick(info.layerMask, _workResults)) {
                    continue;
                }

                // 衝突が発生していたら通知する
                hitResult.center = info.collision.Center;
                hitResult.customData = info.customData;
                foreach (var result in _workResults) {
                    hitResult.collider = result;
                    info.listener?.OnHitCollision(hitResult);
                }
            }
        }

        /// <summary>
        /// レイキャストコリジョン情報の更新
        /// </summary>
        private void UpdateRaycastCollisionInfos() {
            // 削除済みの物をクリア
            for (var i = _raycastCollisionInfos.Count - 1; i >= 0; i--) {
                if (!_raycastCollisionInfos[i].destroy) {
                    continue;
                }

                // Debug用の登録解除
                CollisionVisualizer.Unregister(_raycastCollisionInfos[i].collision);

                _raycastCollisionInfos.RemoveAt(i);
            }

            // 当たり判定実行
            var hitResult = new RaycastHitResult();
            for (var i = 0; i < _raycastCollisionInfos.Count; i++) {
                var info = _raycastCollisionInfos[i];
                _workRaycastResults.Clear();

                if (!info.collision.Tick(info.layerMask, _workRaycastResults)) {
                    continue;
                }

                // 衝突が発生していたら通知する
                hitResult.customData = info.customData;
                foreach (var result in _workRaycastResults) {
                    hitResult.raycastHit = result;
                    hitResult.hitCount = ++info.hitCount;
                    info.listener?.OnHitRaycastCollision(hitResult);
                }
            }
        }
    }
}