using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Playableを再生させるためのクラス
    /// </summary>
    public class PlayablePlayer : IDisposable {
        /// <summary>
        /// 再生中Provider情報
        /// </summary>
        private struct PlayingInfo {
            public IPlayableProvider provider;

            public void TryDispose(bool force = false) {
                if (provider == null) {
                    return;
                }

                if (provider.AutoDispose || force) {
                    provider.Dispose();
                    provider = null;
                }
            }
        }

        // Playable情報
        private PlayableGraph _graph;
        private readonly AnimationMixerPlayable _mixer;

        // 再生速度
        private float _speed = 1.0f;

        // 再生に使っているProvider情報
        private PlayingInfo _prevPlayingInfo;
        private PlayingInfo _currentPlayingInfo;

        // 再生に使う時間情報
        private float _blendDuration;
        private float _blendTime;
        private float _prevTime;
        private float _currentTime;

        // アニメーションの更新をSkipするフレーム数(0以上)
        public int SkipFrame { get; set; } = 0;
        // アニメーションの更新をSkipするかのフレーム数に対するOffset
        public int SkipFrameOffset { get; set; } = 0;
        // AnimationJob差し込み用
        public AnimationJobPlayer JobPlayer { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="animator">Outputを反映させるAnimator</param>
        /// <param name="updateMode">更新モード</param>
        /// <param name="outputSortingOrder">Outputの出力オーダー</param>
        public PlayablePlayer(Animator animator, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime,
            ushort outputSortingOrder = 0) {
            _graph = PlayableGraph.Create($"{nameof(PlayablePlayer)}({animator.name})");
            _mixer = AnimationMixerPlayable.Create(_graph, 2);
            var output = AnimationPlayableOutput.Create(_graph, "Output", animator);

            _graph.SetTimeUpdateMode(updateMode);
            output.SetSortingOrder(outputSortingOrder);
            output.SetSourcePlayable(_mixer);
            _graph.Play();

            _prevPlayingInfo = new PlayingInfo();
            _currentPlayingInfo = new PlayingInfo();
            
            // JobPlayerの生成
            JobPlayer = new AnimationJobPlayer(animator, _graph);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // JobPlayer削除
            JobPlayer.Dispose();
            
            // 再生中のPlayableをクリア
            _prevPlayingInfo.TryDispose();
            _currentPlayingInfo.TryDispose();

            // Graphを削除
            _graph.Destroy();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            // AnimationSkip対応
            var graphPlaying = _graph.IsPlaying();
            if (SkipFrame <= 0 || (Time.frameCount + SkipFrameOffset) % (SkipFrame + 1) == 0) {
                if (!graphPlaying) {
                    _graph.Play();
                }
            }
            else {
                if (graphPlaying) {
                    _graph.Stop();
                }
            }

            // 廃棄状態の更新
            if (_prevPlayingInfo.provider != null) {
                if (_prevPlayingInfo.provider.IsDisposed) {
                    _prevPlayingInfo.provider = null;
                    _blendTime = 0.0f;
                    _blendDuration = 0.0f;

                    // 接続状態更新
                    _mixer.DisconnectInput(0);
                    _mixer.DisconnectInput(1);
                    if (_currentPlayingInfo.provider != null) {
                        _mixer.ConnectInput(0, _currentPlayingInfo.provider.GetPlayable(), 0);
                        _mixer.SetInputWeight(0, 1.0f);
                    }
                }
            }

            if (_currentPlayingInfo.provider != null) {
                if (_currentPlayingInfo.provider.IsDisposed) {
                    _currentPlayingInfo.provider = null;
                }
            }

            // 時間の更新
            var updateMode = _graph.GetTimeUpdateMode();
            var deltaTime =
                (updateMode == DirectorUpdateMode.UnscaledGameTime ? Time.unscaledDeltaTime : Time.deltaTime) * _speed;

            _prevTime += deltaTime;
            _currentTime += deltaTime;
            _blendTime += deltaTime;

            // Blend中
            if (_blendTime < _blendDuration) {
                var rate = _blendTime / _blendDuration;
                _mixer.SetInputWeight(0, 1.0f - rate);
                if (_currentPlayingInfo.provider != null) {
                    _mixer.SetInputWeight(1, rate);
                }
            }
            // Blend完了
            else if (_prevPlayingInfo.provider != null) {
                // Providerの削除
                _prevPlayingInfo.TryDispose();

                // Mixerの接続関係修正
                _mixer.DisconnectInput(0);
                _mixer.DisconnectInput(1);
                if (_currentPlayingInfo.provider != null) {
                    _mixer.ConnectInput(0, _currentPlayingInfo.provider.GetPlayable(), 0);
                    _mixer.SetInputWeight(0, 1.0f);
                }
            }

            // PlayableProviderの更新
            void UpdateProvider(IPlayableProvider provider, float time) {
                if (provider == null) {
                    return;
                }

                provider.SetTime(time);
                provider.SetSpeed(_speed);
            }

            UpdateProvider(_prevPlayingInfo.provider, _prevTime);
            UpdateProvider(_currentPlayingInfo.provider, _currentTime);
            
            // JobProvider更新
            JobPlayer.Update();

            // Manualモードの場合、ここで骨の更新を行う
            if (updateMode == DirectorUpdateMode.Manual) {
                _graph.Evaluate(deltaTime);
            }
        }

        /// <summary>
        /// 更新モードの変更
        /// </summary>
        public void SetUpdateMode(DirectorUpdateMode updateMode) {
            _graph.SetTimeUpdateMode(updateMode);
        }

        /// <summary>
        /// 再生対象のPlayableProviderを変更
        /// </summary>
        /// <param name="provider">変更対象のPlayableを返すProvider</param>
        /// <param name="blendDuration">ブレンド時間</param>
        public void Change(IPlayableProvider provider, float blendDuration) {
            // 無効なProvider
            if (provider != null && provider.IsDisposed) {
                Debug.LogError($"Provider is invalid. [{provider}]");
                return;
            }

            // ブレンド中なら古い物は削除
            _prevPlayingInfo.TryDispose();

            // CurrentをPrevに移す
            _prevPlayingInfo.provider = _currentPlayingInfo.provider;
            _prevTime = _currentTime;

            // 未初期化だった場合は、初期化
            if (provider != null && !provider.IsInitialized) {
                provider.Initialize(_graph);
            }

            _currentPlayingInfo.provider = provider;
            _currentTime = 0.0f;

            // Graphの更新
            _mixer.DisconnectInput(0);
            _mixer.DisconnectInput(1);
            if (_prevPlayingInfo.provider != null) {
                _mixer.ConnectInput(0, _prevPlayingInfo.provider.GetPlayable(), 0);
                _mixer.SetInputWeight(0, 1.0f);
                if (_currentPlayingInfo.provider != null) {
                    _mixer.ConnectInput(1, _currentPlayingInfo.provider.GetPlayable(), 0);
                    _mixer.SetInputWeight(1, 0.0f);
                }
            }
            else if (_currentPlayingInfo.provider != null) {
                _mixer.ConnectInput(0, _currentPlayingInfo.provider.GetPlayable(), 0);
                _mixer.SetInputWeight(0, 1.0f);
            }

            // ブレンド時間初期化
            if (_prevPlayingInfo.provider != null) {
                _blendDuration = blendDuration;
                _blendTime = 0.0f;
            }
            else {
                _blendDuration = 0.0f;
                _blendTime = 0.0f;
            }
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            JobPlayer.SetSpeed(speed);
            
            if (Math.Abs(speed - _speed) <= float.Epsilon) {
                return;
            }

            _speed = Mathf.Max(0.0f, speed);
        }
    }
}