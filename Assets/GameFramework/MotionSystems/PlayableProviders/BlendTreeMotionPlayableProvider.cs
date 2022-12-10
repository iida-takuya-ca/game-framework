using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems {
    /// <summary>
    /// 複数のClipをブレンド再生するPlayable用のProvider
    /// </summary>
    public class BlendTreeMotionPlayableProvider : MotionPlayableProvider {
        // ブレンド用クリップ情報
        [Serializable]
        public struct ClipInfo {
            public AnimationClip clip;
            [Range(0.0f, 1.0f)] public float beginRate;
            [Range(0.0f, 1.0f)] public float endRate;
        }

        private AnimationMixerPlayable _mixerPlayable;
        private AnimationClipPlayable[] _clipPlayables = new AnimationClipPlayable[0];
        private ClipInfo[] _clipInfos = new ClipInfo[0];
        private float _blendRate = 0.0f;

        protected override Playable Playable => _mixerPlayable;

        // ブレンド割合(0〜1)
        public float BlendRate {
            get => _blendRate;
            set {
                _blendRate = Mathf.Clamp01(value);
                RefreshInputWeights();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BlendTreeMotionPlayableProvider(bool autoDispose, params ClipInfo[] clipInfos)
            : base(autoDispose) {
            _clipInfos = clipInfos;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BlendTreeMotionPlayableProvider(bool autoDispose, params AnimationClip[] clips)
            : base(autoDispose) {
            _clipInfos = new ClipInfo[clips.Length];

            var unitRate = 1.0f / clips.Length;
            for (var i = 0; i < _clipInfos.Length; i++) {
                var info = new ClipInfo {
                    clip = clips[i],
                    beginRate = i * unitRate,
                    endRate = (i + 1) * unitRate
                };
                _clipInfos[i] = info;
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(PlayableGraph graph) {
            _clipPlayables = _clipInfos.Select(x => AnimationClipPlayable.Create(graph, x.clip)).ToArray();
            _mixerPlayable = AnimationMixerPlayable.Create(graph, _clipInfos.Length);
            for (var i = 0; i < _clipPlayables.Length; i++) {
                _mixerPlayable.ConnectInput(i, _clipPlayables[i], 0);
            }

            RefreshInputWeights();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _mixerPlayable.Destroy();
            for (var i = 0; i < _clipPlayables.Length; i++) {
                _clipPlayables[i].Destroy();
            }

            _clipPlayables = new AnimationClipPlayable[0];
            _clipInfos = new ClipInfo[0];
        }

        /// <summary>
        /// ブレンド率をPlayableに反映
        /// </summary>
        private void RefreshInputWeights() {
            for (var i = 0; i < _clipPlayables.Length; i++) {
                var blendClip = _clipInfos[i];
                var weight = blendClip.endRate > blendClip.beginRate
                    ? (_blendRate - blendClip.beginRate) / (blendClip.endRate - blendClip.beginRate)
                    : 0.0f;
                _mixerPlayable.SetInputWeight(i, weight);
            }
        }
    }
}