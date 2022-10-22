using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.MotionSystems {
    /// <summary>
    /// モーション再生用クラス
    /// </summary>
    public class MotionPlayer : IDisposable {
        /// <summary>
        /// AnimationJob管理用ハンドル
        /// </summary>
        public struct AnimationJobHandle {
            private readonly AnimationJobInfo _info;

            public bool IsValid => _info != null && !_info.dispose && _info.playable.IsValid();

            public AnimationJobHandle(AnimationJobInfo info) {
                _info = info;
            }

            public override bool Equals(object obj) {
                if (obj is AnimationJobHandle handle) {
                    return handle._info == _info;
                }

                return false;
            }

            public override int GetHashCode() {
                return _info != null ? _info.GetHashCode() : 0;
            }
        }

        /// <summary>
        /// AnimationJob情報
        /// </summary>
        public class AnimationJobInfo {
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
        private RootScaleAnimationJobHandler _rootScaleAnimationJobHandler;

        private float _blendDuration;
        private float _blendTime;
        private float _prevTime;
        private float _currentTime;

        // アニメーションの更新をSkipするフレーム数(0以上)
        public int SkipFrame { get; set; } = 0;
        // アニメーションの更新をSkipするかのフレーム数に対するOffset
        public int SkipFrameOffset { get; set; } = 0;
        
        // ルートスケール（座標）
        public Vector3 RootPositionScale {
            get => _rootScaleAnimationJobHandler.PositionScale;
            set => _rootScaleAnimationJobHandler.PositionScale = value;
        }
        // ルートスケール（回転）
        public Vector3 RootAngleScale {
            get => _rootScaleAnimationJobHandler.AngleScale;
            set => _rootScaleAnimationJobHandler.AngleScale = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MotionPlayer(Animator animator, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime) {
            _graph = PlayableGraph.Create($"{animator.name}:MotionGraph");
            _output = AnimationPlayableOutput.Create(_graph, "Output", animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 2, true);

            _graph.SetTimeUpdateMode(updateMode);
            _output.SetSourcePlayable(_mixer);
            _graph.Play();
            
            // RootScaleJobの初期化
            _rootScaleAnimationJobHandler = new RootScaleAnimationJobHandler();
            AddJob(_rootScaleAnimationJobHandler);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // AnimationJobを全クリア
            foreach (var info in _animationJobInfos) {
                info.dispose = true;
            }

            RefreshAnimationJobInfos();

            _graph.Destroy();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
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

            // JobInfoの更新
            RefreshAnimationJobInfos();

            // 時間の更新
            var updateMode = _graph.GetTimeUpdateMode();
            _prevTime += deltaTime;
            _currentTime += deltaTime;
            _blendTime += deltaTime;

            // Blend
            if (_blendTime < _blendDuration) {
                // Blend中
                var rate = _blendTime / _blendDuration;
                _mixer.SetInputWeight(0, 1.0f - rate);
                if (_currentMotionPlayableHandler != null) {
                    _mixer.SetInputWeight(1, rate);
                }
            }
            else if (_prevMotionPlayableHandler != null) {
                // Blend完了
                _prevMotionPlayableHandler.Dispose();
                _prevMotionPlayableHandler = null;

                // Mixerの接続関係修正
                _mixer.DisconnectInput(0);
                _mixer.DisconnectInput(1);
                if (_currentMotionPlayableHandler != null) {
                    _mixer.ConnectInput(0, _currentMotionPlayableHandler.Playable, 0);
                    _mixer.SetInputWeight(0, 1.0f);
                }
            }

            // 速度反映の計算
            var baseDeltaTime = deltaTime;
            switch (updateMode) {
                case DirectorUpdateMode.Manual:
                    break;
                case DirectorUpdateMode.GameTime:
                    baseDeltaTime = Time.deltaTime;
                    break;
                case DirectorUpdateMode.UnscaledGameTime:
                    baseDeltaTime = Time.unscaledDeltaTime;
                    break;
                case DirectorUpdateMode.DSPClock:
                    baseDeltaTime = Time.deltaTime;
                    break;
            }

            var playableSpeed = 0.0f;
            if (baseDeltaTime > float.Epsilon) {
                playableSpeed = deltaTime / baseDeltaTime;
            }

            void UpdateHandler(IMotionPlayableHandler handler, float time) {
                if (handler == null) {
                    return;
                }

                handler.Playable.SetTime(time);
                handler.Playable.SetSpeed(playableSpeed);
            }

            // Handlerの更新
            UpdateHandler(_prevMotionPlayableHandler, _prevTime);
            UpdateHandler(_currentMotionPlayableHandler, _currentTime);
            
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
        /// モーションの設定
        /// </summary>
        public void SetMotion(IMotionPlayableHandler handler, float blendDuration) {
            // Providerの更新
            _prevMotionPlayableHandler?.Dispose();
            _prevMotionPlayableHandler = _currentMotionPlayableHandler;
            _prevTime = _currentTime;

            if (handler != null) {
                handler.Initialize(_graph);
            }

            _currentMotionPlayableHandler = handler;
            _currentTime = 0.0f;

            // Graphの更新
            _mixer.DisconnectInput(0);
            _mixer.DisconnectInput(1);
            if (_prevMotionPlayableHandler != null) {
                _mixer.ConnectInput(0, _prevMotionPlayableHandler.Playable, 0);
                _mixer.SetInputWeight(0, 1.0f);
                if (_currentMotionPlayableHandler != null) {
                    _mixer.ConnectInput(1, _currentMotionPlayableHandler.Playable, 0);
                    _mixer.SetInputWeight(1, 0.0f);
                }
            }
            else if (_currentMotionPlayableHandler != null) {
                _mixer.ConnectInput(0, _currentMotionPlayableHandler.Playable, 0);
                _mixer.SetInputWeight(0, 1.0f);
            }

            // ブレンド時間初期化
            if (_prevMotionPlayableHandler != null) {
                _blendDuration = blendDuration;
                _blendTime = 0.0f;
            }
            else {
                _blendDuration = 0.0f;
                _blendTime = 0.0f;
            }
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(AnimationClip clip, float blendDuration) {
            if (clip == null) {
                ResetMotion(blendDuration);
                return;
            }

            SetMotion(new SingleMotionPlayableHandler(clip), blendDuration);
        }

        /// <summary>
        /// モーションの設定
        /// </summary>
        public void SetMotion(RuntimeAnimatorController controller, float blendDuration) {
            if (controller == null) {
                ResetMotion(blendDuration);
                return;
            }

            SetMotion(new AnimatorControllerMotionPlayableHandler(controller), blendDuration);
        }

        /// <summary>
        /// モーションのリセット
        /// </summary>
        public void ResetMotion(float blendDuration) {
            SetMotion(default(IMotionPlayableHandler), blendDuration);
        }

        /// <summary>
        /// Jobの追加
        /// </summary>
        public AnimationJobHandle AddJob<T>(IAnimationJobHandler<T> handler)
            where T : struct, IAnimationJob {
            // Jobの生成＆Playable登録
            var job = handler.Initialize(this);
            var playable = AnimationScriptPlayable.Create(_graph, job);
            playable.SetInputCount(1);
            var jobInfo = new AnimationJobInfo {
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
        public void RemoveJob(AnimationJobHandle handle) {
            var jobInfo = _animationJobInfos.FirstOrDefault(x => x.GetHashCode() == handle.GetHashCode());
            if (jobInfo == null || jobInfo.dispose) {
                return;
            }

            jobInfo.dispose = true;
        }

        /// <summary>
        /// 後処理用のAnimationJobを更新
        /// </summary>
        private void RefreshPostProcess() {
            if (_animationJobInfos.Count > 0) {
                // ScriptPlayableをOutputとMixerの間に差し込む
                var outputPlayable = _animationJobInfos[_animationJobInfos.Count - 1].playable;
                _output.SetSourcePlayable(outputPlayable);

                for (var i = _animationJobInfos.Count - 2; i >= 0; i--) {
                    var inputPlayable = _animationJobInfos[i].playable;
                    outputPlayable.ConnectInput(0, inputPlayable, 0);
                    outputPlayable = inputPlayable;
                }

                outputPlayable.ConnectInput(0, _mixer, 0);
            }
            else {
                // 対象がなければ、Mixerを直接Outputする
                _output.SetSourcePlayable(_mixer);
            }
        }

        /// <summary>
        /// AnimationJobInfoリストの更新
        /// </summary>
        private void RefreshAnimationJobInfos() {
            var dirty = false;

            // AnimationJobInfoの廃棄済みな物を削除
            for (var i = _animationJobInfos.Count - 1; i >= 0; i--) {
                var info = _animationJobInfos[i];
                if (!info.dispose) {
                    continue;
                }

                info.playable.Destroy();
                info.disposable.Dispose();
                _animationJobInfos.RemoveAt(i);
                dirty = true;
            }

            if (dirty) {
                RefreshPostProcess();
            }
        }
    }
}