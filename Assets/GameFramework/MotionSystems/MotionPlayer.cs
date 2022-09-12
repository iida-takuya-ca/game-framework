using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems
{
    /// <summary>
    /// モーション再生用クラス
    /// </summary>
    public class MotionPlayer : IDisposable
    {
        /// <summary>
        /// AnimationJob管理用ハンドル
        /// </summary>
        public struct AnimationJobHandle
        {
            private readonly AnimationJobInfo _info;
            
            public bool IsValid => _info != null && !_info.dispose && _info.playable.IsValid();

            public AnimationJobHandle(AnimationJobInfo info)
            {
                _info = info;
            }
            
            public override bool Equals(object obj)
            {
                if (obj is AnimationJobHandle handle)
                {
                    return handle._info == _info;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return _info != null ? _info.GetHashCode() : 0;
            }
        }
        
        /// <summary>
        /// AnimationJob情報
        /// </summary>
        public class AnimationJobInfo
        {
            public AnimationScriptPlayable playable;
            public IDisposable disposable;
            public bool dispose;
        }
        
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationMixerPlayable _mixer;
        private List<AnimationJobInfo> _animationJobInfos = new List<AnimationJobInfo>();
        private IMotionPlayableHandler _prevMotionPlayableHandler;
        private IMotionPlayableHandler _currentMotionPlayableHandler;

        private float _blendDuration;
        private float _blendTime;
        private float _prevTime;
        private float _currentTime;

        // アニメーションの更新をSkipするフレーム数(0以上)
        public int SkipFrame { get; set; } = 0;
        // アニメーションの更新をSkipするかのフレーム数に対するOffset
        public int SkipFrameOffset { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MotionPlayer(Animator animator)
        {
            _graph = PlayableGraph.Create($"{animator.name}:MotionGraph");
            _output = AnimationPlayableOutput.Create(_graph, "Output", animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 2, true);

            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _output.SetSourcePlayable(_mixer);
            _graph.Play();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose()
        {
            // AnimationJobを全クリア
            foreach (var info in _animationJobInfos)
            {
                info.dispose = true;
            }
            RefreshAnimationJobInfos();
            
            _graph.Destroy();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime)
        {
            // AnimationSkip対応
            var graphPlaying = _graph.IsPlaying();
            if (SkipFrame <= 0 || (Time.frameCount + SkipFrameOffset) % (SkipFrame + 1) == 0)
            {
                if (!graphPlaying)
                {
                    _graph.Play();
                }
            }
            else
            {
                if (graphPlaying)
                {
                    _graph.Stop();
                }
            }

            // JobInfoの更新
            RefreshAnimationJobInfos();
            
            // 時間の更新
            _prevTime += deltaTime;
            _currentTime += deltaTime;
            _blendTime += deltaTime;

            void UpdateProvider(IMotionPlayableHandler provider, float time)
            {
                if (provider == null)
                {
                    return;
                }

                provider.SetTime(time);
            }

            // Providerの更新
            UpdateProvider(_prevMotionPlayableHandler, _prevTime);
            UpdateProvider(_currentMotionPlayableHandler, _currentTime);

            // Blend
            if (_blendTime < _blendDuration)
            {
                // Blend中
                var rate = _blendTime / _blendDuration;
                _mixer.SetInputWeight(0, 1.0f - rate);
                _mixer.SetInputWeight(1, rate);
            }
            else if (_prevMotionPlayableHandler != null)
            {
                // Blend完了
                _prevMotionPlayableHandler.Dispose();
                _prevMotionPlayableHandler = null;

                // Mixerの接続関係修正
                _mixer.DisconnectInput(0);
                _mixer.DisconnectInput(1);
                _mixer.ConnectInput(0, _currentMotionPlayableHandler.Playable, 0);
                _mixer.SetInputWeight(0, 1.0f);
            }
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(IMotionPlayableHandler handler, float blendDuration)
        {
            // Providerの更新
            _prevMotionPlayableHandler?.Dispose();
            _prevMotionPlayableHandler = _currentMotionPlayableHandler;
            _prevTime = _currentTime;

            handler.Initialize(_graph);
            _currentMotionPlayableHandler = handler;
            _currentTime = 0.0f;

            // Graphの更新
            _mixer.DisconnectInput(0);
            _mixer.DisconnectInput(1);
            if (_prevMotionPlayableHandler != null)
            {
                _mixer.ConnectInput(0, _prevMotionPlayableHandler.Playable, 0);
                _mixer.ConnectInput(1, _currentMotionPlayableHandler.Playable, 0);
                _mixer.SetInputWeight(0, 1.0f);
                _mixer.SetInputWeight(1, 0.0f);
            }
            else
            {
                _mixer.ConnectInput(0, _currentMotionPlayableHandler.Playable, 0);
                _mixer.SetInputWeight(0, 1.0f);
            }

            // ブレンド時間初期化
            if (_prevMotionPlayableHandler != null)
            {
                _blendDuration = blendDuration;
                _blendTime = 0.0f;
            }
            else
            {
                _blendDuration = 0.0f;
                _blendTime = 0.0f;
            }
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(AnimationClip clip, float blendDuration)
        {
            SetMotion(new SingleMotionPlayableHandler(clip), blendDuration);
        }
        
        /// <summary>
        /// Jobの追加
        /// </summary>
        public AnimationJobHandle AddJob<T>(IAnimationJobHandler<T> handler)
        where T : struct, IAnimationJob
        {
            // Jobの生成＆Playable登録
            var job = handler.Initialize(this);
            var playable = AnimationScriptPlayable.Create(_graph, job);
            playable.SetInputCount(1);
            var jobInfo = new AnimationJobInfo
            {
                playable = playable,
                disposable = handler
            };
            _animationJobInfos.Add(jobInfo);

            // PostProcessの更新
            RefreshPostProcess();
            
            return new AnimationJobHandle(jobInfo);
        }

        /// <summary>
        /// Jobの削除
        /// </summary>
        public void RemoveJob(AnimationJobHandle handle)
        {
            var jobInfo = _animationJobInfos.FirstOrDefault(x => x.GetHashCode() == handle.GetHashCode());
            if (jobInfo == null || jobInfo.dispose)
            {
                return;
            }

            jobInfo.dispose = true;
        }

        /// <summary>
        /// 後処理用のAnimationJobを更新
        /// </summary>
        private void RefreshPostProcess()
        {
            if (_animationJobInfos.Count > 0)
            {
                // ScriptPlayableをOutputとMixerの間に差し込む
                var outputPlayable = _animationJobInfos[_animationJobInfos.Count - 1].playable;
                _output.SetSourcePlayable(outputPlayable);
                
                for (var i = _animationJobInfos.Count - 2; i >= 0; i--)
                {
                    var inputPlayable = _animationJobInfos[i].playable;
                    outputPlayable.ConnectInput(0, inputPlayable, 0);
                    outputPlayable = inputPlayable;
                }
                
                outputPlayable.ConnectInput(0, _mixer, 0);
            }
            else
            {
                // 対象がなければ、Mixerを直接Outputする
                _output.SetSourcePlayable(_mixer);
            }
        }

        /// <summary>
        /// AnimationJobInfoリストの更新
        /// </summary>
        private void RefreshAnimationJobInfos()
        {
            var dirty = false;
            
            // AnimationJobInfoの廃棄済みな物を削除
            for (var i = _animationJobInfos.Count - 1; i >= 0; i--)
            {
                var info = _animationJobInfos[i];
                if (!info.dispose)
                {
                    continue;
                }
                
                info.playable.Destroy();
                info.disposable.Dispose();
                _animationJobInfos.RemoveAt(i);
                dirty = true;
            }

            if (dirty)
            {
                RefreshPostProcess();
            }
        }
    }
}