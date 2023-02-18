using System;
using GameFramework.TaskSystems;
using UnityEngine;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 飛翔体管理用クラス
    /// </summary>
    public class ProjectileManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 更新モード
        /// </summary>
        public enum UpdateMode {
            Update,
            LateUpdate,
        }
        
        /// <summary>
        /// 飛翔ハンドル
        /// </summary>
        public struct ProjectileHandle : IDisposable {
            private ProjectileManager _manager;
            private PlayingInfo _playingInfo;

            // 有効なハンドルか
            public bool IsValid => _manager != null && _playingInfo != null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal ProjectileHandle(ProjectileManager manager, PlayingInfo info) {
                _manager = manager;
                _playingInfo = info;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (!IsValid) {
                    return;
                }

                // 再生中なら止める
                if (!_playingInfo.stopped) {
                    _playingInfo.Stop();
                    var infoIndex = _manager._playingInfos.IndexOf(_playingInfo);
                    if (infoIndex >= 0) {
                        _manager._removePlayingInfoIndices.Add(infoIndex);
                    }
                }

                _playingInfo = null;
                _manager = null;
            }
        }

        /// <summary>
        /// 再生情報
        /// </summary>
        internal class PlayingInfo {
            public IProjectile projectile;
            public bool started;
            public bool stopped;

            public Action<Vector3, Quaternion> onUpdatedTransform;
            public Action onStopped;

            /// <summary>
            /// 開始処理
            /// </summary>
            public void Start() {
                if (started) {
                    return;
                }
                
                projectile?.Start();
                started = true;
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop() {
                if (stopped) {
                    return;
                }

                projectile?.Stop();
                stopped = true;
                onStopped?.Invoke();
            }
        }

        // 更新モード
        private UpdateMode _updateMode;
        // 飛翔体再生情報リスト
        private List<PlayingInfo> _playingInfos = new List<PlayingInfo>();
        // リスト除外対象Indexのワーク
        private List<int> _removePlayingInfoIndices = new List<int>();

        // DeltaTime管理用クラス
        public LayeredTime LayeredTime { get; } = new LayeredTime();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="updateMode">更新モード</param>
        public ProjectileManager(UpdateMode updateMode = UpdateMode.Update) {
            _updateMode = updateMode;
        }

        /// <summary>
        /// 飛翔体の開始
        /// </summary>
        /// <param name="projectile">飛翔体インスタンス</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onStopped">飛翔完了通知</param>
        public ProjectileHandle Play(IProjectile projectile,
            Action<Vector3, Quaternion> onUpdatedTransform,
            Action onStopped) {
            var playingInfo = new PlayingInfo {
                projectile = projectile,
                onUpdatedTransform = onUpdatedTransform,
                onStopped = onStopped
            };
            _playingInfos.Add(playingInfo);

            var handle = new ProjectileHandle(this, playingInfo);
            return handle;
        }

        /// <summary>
        /// 飛翔体の停止
        /// </summary>
        public void Stop(ProjectileHandle handle) {
            handle.Dispose();
        }

        /// <summary>
        /// 全飛翔体の停止
        /// </summary>
        public void StopAll() {
            for (var i = 0; i < _playingInfos.Count; i++) {
                var info = _playingInfos[i];
                info.Stop();
            }

            _playingInfos.Clear();
            _removePlayingInfoIndices.Clear();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            // 全飛翔体を停止
            StopAll();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            if (_updateMode == UpdateMode.Update) {
                UpdatePlayingInfos();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            if (_updateMode == UpdateMode.LateUpdate) {
                UpdatePlayingInfos();
            }
        }

        /// <summary>
        /// Projectileの更新処理
        /// </summary>
        private void UpdatePlayingInfos() {
            var deltaTime = LayeredTime.DeltaTime;

            for (var i = 0; i < _playingInfos.Count; i++) {
                var playingInfo = _playingInfos[i];

                if (playingInfo.stopped) {
                    continue;
                }

                // 開始していなければStart処理実行
                playingInfo.Start();

                // 更新処理
                var continuation = playingInfo.projectile.Update(deltaTime);
                playingInfo.onUpdatedTransform?.Invoke(playingInfo.projectile.Position,
                    playingInfo.projectile.Rotation);

                if (!continuation) {
                    // 終了したらStop処理実行
                    playingInfo.Stop();
                    _removePlayingInfoIndices.Add(i);
                }
            }

            // 不要なProjectileの再生情報をクリア
            for (var i = _removePlayingInfoIndices.Count - 1; i >= 0; i--) {
                _playingInfos.RemoveAt(_removePlayingInfoIndices[i]);
            }

            _removePlayingInfoIndices.Clear();
        }
    }
}