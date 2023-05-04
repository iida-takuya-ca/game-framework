using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Motionを再生させるためのクラス
    /// </summary>
    public class MotionPlayer : IDisposable {
        /// <summary>
        /// レイヤー設定
        /// </summary>
        [Serializable]
        public struct LayerSetting {
            [Tooltip("加算レイヤーか")]
            public bool additive;
            [Tooltip("アバターマスク")]
            public AvatarMask avatarMask;
        }

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

        /// <summary>
        /// Layer情報
        /// </summary>
        private class LayerInfo {
            // 再生に使うミキサー
            public AnimationMixerPlayable mixer;

            // 再生に使っているProvider情報
            public PlayingInfo prevPlayingInfo;
            public PlayingInfo currentPlayingInfo;

            // 再生に使う時間情報
            public float blendDuration;
            public float blendTime;
            public float prevTime;
            public float currentTime;
        }

        // Playable情報
        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _layerMixer;

        // 再生速度
        private float _speed = 1.0f;

        // ベースLayer情報
        private LayerInfo _baseLayerInfo;
        // 追加Layer情報
        private List<LayerInfo> _additiveLayerInfos = new List<LayerInfo>();

        // 再生に使っているProvider情報
        // private PlayingInfo _prevPlayingInfo;
        // private PlayingInfo _currentPlayingInfo;
        //
        // // 再生に使う時間情報
        // private float _blendDuration;
        // private float _blendTime;
        // private float _prevTime;
        // private float _currentTime;

        // アニメーションの更新をSkipするフレーム数(0以上)
        public int SkipFrame { get; set; } = 0;
        // アニメーションの更新をSkipするかのフレーム数に対するOffset
        public int SkipFrameOffset { get; set; } = 0;
        // AnimationJob差し込み用
        public AnimationJobConnector JobConnector { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="animator">Outputを反映させるAnimator</param>
        /// <param name="updateMode">更新モード</param>
        /// <param name="outputSortingOrder">Outputの出力オーダー</param>
        public MotionPlayer(Animator animator, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime,
            ushort outputSortingOrder = 0) {
            _graph = PlayableGraph.Create($"{nameof(MotionPlayer)}({animator.name})");
            _layerMixer = AnimationLayerMixerPlayable.Create(_graph, 1);
            var output = AnimationPlayableOutput.Create(_graph, "Output", animator);

            _graph.SetTimeUpdateMode(updateMode);
            output.SetSortingOrder(outputSortingOrder);
            output.SetSourcePlayable(_layerMixer);
            _graph.Play();

            // 基本Layerの作成
            _baseLayerInfo = CreateLayerInfo();

            // Layerの接続
            _layerMixer.ConnectInput(0, _baseLayerInfo.mixer, 0);
            _layerMixer.SetLayerAdditive(0, false);
            _layerMixer.SetInputWeight(0, 1.0f);

            // JobPlayerの生成
            JobConnector = new AnimationJobConnector(animator, _graph);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // JobConnector削除
            JobConnector.Dispose();

            // Layerの削除
            foreach (var layerInfo in _additiveLayerInfos) {
                DestroyLayerInfo(layerInfo);
            }

            _additiveLayerInfos.Clear();
            DestroyLayerInfo(_baseLayerInfo);
            _baseLayerInfo = null;

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

            // 変位時間取得
            var updateMode = _graph.GetTimeUpdateMode();
            var deltaTime =
                (updateMode == DirectorUpdateMode.UnscaledGameTime ? Time.unscaledDeltaTime : Time.deltaTime) *
                _speed;

            // PlayableProviderの更新
            void UpdateProvider(IPlayableProvider provider, float time) {
                if (provider == null) {
                    return;
                }

                provider.SetTime(time);
                provider.SetSpeed(_speed);
            }

            // LayerInfoの更新
            void UpdateLayerInfo(LayerInfo layerInfo) {
                // 廃棄状態の更新
                if (layerInfo.prevPlayingInfo.provider != null) {
                    if (layerInfo.prevPlayingInfo.provider.IsDisposed) {
                        layerInfo.prevPlayingInfo.provider = null;
                        layerInfo.blendTime = 0.0f;
                        layerInfo.blendDuration = 0.0f;

                        // 接続状態更新
                        layerInfo.mixer.DisconnectInput(0);
                        layerInfo.mixer.DisconnectInput(1);
                        if (layerInfo.currentPlayingInfo.provider != null) {
                            layerInfo.mixer.ConnectInput(0, layerInfo.currentPlayingInfo.provider.GetPlayable(), 0);
                            layerInfo.mixer.SetInputWeight(0, 1.0f);
                        }
                    }
                }

                if (layerInfo.currentPlayingInfo.provider != null) {
                    if (layerInfo.currentPlayingInfo.provider.IsDisposed) {
                        layerInfo.currentPlayingInfo.provider = null;
                    }
                }

                // 時間の更新
                layerInfo.prevTime += deltaTime;
                layerInfo.currentTime += deltaTime;
                layerInfo.blendTime += deltaTime;

                // Blend中
                if (layerInfo.blendTime < layerInfo.blendDuration) {
                    var rate = layerInfo.blendTime / layerInfo.blendDuration;
                    layerInfo.mixer.SetInputWeight(0, 1.0f - rate);
                    if (layerInfo.currentPlayingInfo.provider != null) {
                        layerInfo.mixer.SetInputWeight(1, rate);
                    }
                }
                // Blend完了
                else if (layerInfo.prevPlayingInfo.provider != null) {
                    // Providerの削除
                    layerInfo.prevPlayingInfo.TryDispose();

                    // Mixerの接続関係修正
                    layerInfo.mixer.DisconnectInput(0);
                    layerInfo.mixer.DisconnectInput(1);
                    if (layerInfo.currentPlayingInfo.provider != null) {
                        layerInfo.mixer.ConnectInput(0, layerInfo.currentPlayingInfo.provider.GetPlayable(), 0);
                        layerInfo.mixer.SetInputWeight(0, 1.0f);
                    }
                }

                UpdateProvider(layerInfo.prevPlayingInfo.provider, layerInfo.prevTime);
                UpdateProvider(layerInfo.currentPlayingInfo.provider, layerInfo.currentTime);
            }

            // レイヤーの更新
            UpdateLayerInfo(_baseLayerInfo);
            foreach (var layerInfo in _additiveLayerInfos) {
                UpdateLayerInfo(layerInfo);
            }

            // JobProvider更新
            JobConnector.Update(deltaTime);

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
        /// 追加レイヤーの構築
        /// </summary>
        /// <param name="layerSettings">Base以外に構築するLayer設定</param>
        public void BuildAdditionalLayers(params LayerSetting[] layerSettings) {
            // 既にあるLayerInfoの削除
            for (var i = _additiveLayerInfos.Count - 1; i >= 0; i--) {
                DestroyLayerInfo(_additiveLayerInfos[i]);
                _layerMixer.DisconnectInput(i + 1);
            }

            _additiveLayerInfos.Clear();

            // LayerMixerの数をそろえる
            _layerMixer.SetInputCount(layerSettings.Length + 1);

            // 加算用Layerの追加
            for (var i = 0; i < layerSettings.Length; i++) {
                // LayerInfoを作成
                var layerInfo = CreateLayerInfo();

                // Layerの接続
                var index = (uint)(i + 1);
                _layerMixer.ConnectInput(i + 1, layerInfo.mixer, 0);
                _layerMixer.SetLayerAdditive(index, layerSettings[i].additive);
                if (layerSettings[i].avatarMask != null) {
                    _layerMixer.SetLayerMaskFromAvatarMask(index, layerSettings[i].avatarMask);
                }

                _layerMixer.SetInputWeight(i + 1, 1.0f);

                _additiveLayerInfos.Add(layerInfo);
            }
        }

        /// <summary>
        /// 追加用レイヤーのリセット
        /// </summary>
        public void ResetAdditionalLayers() {
            BuildAdditionalLayers(Array.Empty<LayerSetting>());
        }

        /// <summary>
        /// 再生対象のPlayableProviderを変更
        /// </summary>
        /// <param name="layerIndex">対象のLayerIndex</param>
        /// <param name="provider">変更対象のPlayableを返すProvider</param>
        /// <param name="blendDuration">ブレンド時間</param>
        public void Change(int layerIndex, IPlayableProvider provider, float blendDuration) {
            // 無効なProvider
            if (provider != null && provider.IsDisposed) {
                Debug.LogError($"Provider is invalid. [{provider}]");
                return;
            }

            var layerInfo = GetLayerInfo(layerIndex);

            // 無効なLayerIndex
            if (layerInfo == null) {
                Debug.LogError($"Layer is not found. [{layerIndex}]");
                return;
            }

            // 現在再生中のProviderと同じならスキップ
            if (layerInfo.currentPlayingInfo.provider == provider) {
                return;
            }

            // ブレンド中なら古い物は削除
            layerInfo.prevPlayingInfo.TryDispose();

            // CurrentをPrevに移す
            layerInfo.prevPlayingInfo.provider = layerInfo.currentPlayingInfo.provider;
            layerInfo.prevTime = layerInfo.currentTime;

            // 未初期化だった場合は、初期化
            if (provider != null && !provider.IsInitialized) {
                provider.Initialize(_graph);
            }

            layerInfo.currentPlayingInfo.provider = provider;
            layerInfo.currentTime = 0.0f;

            // Graphの更新
            layerInfo.mixer.DisconnectInput(0);
            layerInfo.mixer.DisconnectInput(1);
            if (layerInfo.prevPlayingInfo.provider != null) {
                layerInfo.mixer.ConnectInput(0, layerInfo.prevPlayingInfo.provider.GetPlayable(), 0);
                layerInfo.mixer.SetInputWeight(0, 1.0f);
                if (layerInfo.currentPlayingInfo.provider != null) {
                    layerInfo.mixer.ConnectInput(1, layerInfo.currentPlayingInfo.provider.GetPlayable(), 0);
                    layerInfo.mixer.SetInputWeight(1, 0.0f);
                }
            }
            else if (layerInfo.currentPlayingInfo.provider != null) {
                layerInfo.mixer.ConnectInput(0, layerInfo.currentPlayingInfo.provider.GetPlayable(), 0);
                layerInfo.mixer.SetInputWeight(0, 1.0f);
            }

            // ブレンド時間初期化
            if (layerInfo.prevPlayingInfo.provider != null) {
                layerInfo.blendDuration = blendDuration;
                layerInfo.blendTime = 0.0f;
            }
            else {
                layerInfo.blendDuration = 0.0f;
                layerInfo.blendTime = 0.0f;
            }
        }

        /// <summary>
        /// 再生対象のPlayableProviderを変更
        /// </summary>
        /// <param name="provider">変更対象のPlayableを返すProvider</param>
        /// <param name="blendDuration">ブレンド時間</param>
        public void Change(IPlayableProvider provider, float blendDuration) {
            Change(0, provider, blendDuration);
        }

        /// <summary>
        /// LayerのWeight設定
        /// </summary>
        /// <param name="layerIndex">設定対象のLayerIndex</param>
        /// <param name="weight">再生ウェイト</param>
        public void SetLayerWeight(int layerIndex, float weight) {
            var layerInfo = GetLayerInfo(layerIndex);

            // 無効なLayerIndex
            if (layerInfo == null) {
                Debug.LogError($"Layer is not found. [{layerIndex}]");
                return;
            }

            // ウェイトの設定
            _layerMixer.SetInputWeight(layerIndex, weight);
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            JobConnector.SetSpeed(speed);

            if (Math.Abs(speed - _speed) <= float.Epsilon) {
                return;
            }

            _speed = Mathf.Max(0.0f, speed);
        }

        /// <summary>
        /// Layer情報の取得
        /// </summary>
        private LayerInfo GetLayerInfo(int layerIndex) {
            if (layerIndex < 0 || layerIndex > _additiveLayerInfos.Count) {
                return null;
            }

            return layerIndex == 0 ? _baseLayerInfo : _additiveLayerInfos[layerIndex - 1];
        }

        /// <summary>
        /// Layer情報の作成
        /// </summary>
        private LayerInfo CreateLayerInfo() {
            var layerInfo = new LayerInfo();
            layerInfo.mixer = AnimationMixerPlayable.Create(_graph, 2);
            return layerInfo;
        }

        /// <summary>
        /// Layer情報の削除
        /// </summary>
        private void DestroyLayerInfo(LayerInfo layerInfo) {
            if (layerInfo == null) {
                return;
            }

            _baseLayerInfo = new LayerInfo();
            _baseLayerInfo.mixer = AnimationMixerPlayable.Create(_graph, 2);

            // 再生中のPlayableをクリア
            layerInfo.prevPlayingInfo.TryDispose();
            layerInfo.currentPlayingInfo.TryDispose();

            // Mixerの削除
            layerInfo.mixer.Destroy();
        }
    }
}