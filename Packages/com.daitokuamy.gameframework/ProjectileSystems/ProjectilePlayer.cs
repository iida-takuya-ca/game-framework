using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体再生用クラス
    /// </summary>
    public class ProjectilePlayer : IDisposable {
        /// <summary>
        /// 飛翔ハンドル
        /// </summary>
        public struct Handle : IDisposable {
            private ProjectilePlayer _player;
            private PlayingInfo _playingInfo;

            // 有効なハンドルか
            public bool IsValid => _player != null && _playingInfo != null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(ProjectilePlayer player, PlayingInfo info) {
                _player = player;
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
                    _player._removePlayingInfos.Add(_playingInfo);
                }

                _playingInfo = null;
                _player = null;
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

        // 飛翔体再生情報リスト
        private List<PlayingInfo> _playingInfos = new List<PlayingInfo>();
        // リスト除外対象のワーク
        private List<PlayingInfo> _removePlayingInfos = new List<PlayingInfo>();

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 全飛翔体を停止
            StopAll();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update(float deltaTime) {
            UpdatePlayingInfos(deltaTime);
        }

        /// <summary>
        /// 飛翔体の開始
        /// </summary>
        /// <param name="projectile">飛翔体インスタンス</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onStopped">飛翔完了通知</param>
        public Handle Play(IProjectile projectile,
            Action<Vector3, Quaternion> onUpdatedTransform,
            Action onStopped) {
            var playingInfo = new PlayingInfo {
                projectile = projectile,
                onUpdatedTransform = onUpdatedTransform,
                onStopped = onStopped
            };
            _playingInfos.Add(playingInfo);

            var handle = new Handle(this, playingInfo);
            return handle;
        }

        /// <summary>
        /// 飛翔体の停止
        /// </summary>
        public void Stop(Handle handle) {
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
            _removePlayingInfos.Clear();
        }

        /// <summary>
        /// Projectileの更新処理
        /// </summary>
        private void UpdatePlayingInfos(float deltaTime) {
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
                    _removePlayingInfos.Add(playingInfo);
                }
            }

            // 不要なProjectileの再生情報をクリア
            for (var i = _removePlayingInfos.Count - 1; i >= 0; i--) {
                _playingInfos.Remove(_removePlayingInfos[i]);
            }

            _removePlayingInfos.Clear();
        }
    }
}