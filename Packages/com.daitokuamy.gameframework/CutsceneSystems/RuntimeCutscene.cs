using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace GameFramework.CutsceneSystems {
    /// <summary>
    /// ランタイム用カットシーン
    /// </summary>
    public sealed class RuntimeCutscene : ICutscene {
        private PlayableDirector _playableDirector;
        private bool _isPlaying;

        private List<Object> _bindingTrackKeys = new();

        /// <summary>再生中か</summary>
        bool ICutscene.IsPlaying => _isPlaying;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playableDirector">再生に使うPlayableDirector</param>
        public RuntimeCutscene(PlayableDirector playableDirector) {
            _playableDirector = playableDirector;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICutscene.Initialize() {
            if (_playableDirector == null) {
                return;
            }
            
            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        void IDisposable.Dispose() {
            if (_playableDirector == null) {
                return;
            }

            if (_isPlaying) {
                ((ICutscene)this).Stop();
            }

            _playableDirector = null;
        }
        
        /// <summary>
        /// Poolに戻る際の処理
        /// </summary>
        void ICutscene.OnReturn(){
            if (_playableDirector == null) {
                return;
            }

            if (_isPlaying) {
                ((ICutscene)this).Stop();
            }

            foreach (var trackKey in _bindingTrackKeys) {
                _playableDirector.ClearGenericBinding(trackKey);
            }
            _bindingTrackKeys.Clear();
        }

        /// <summary>
        /// 再生処理
        /// </summary>
        void ICutscene.Play() {
            if (_playableDirector == null) {
                return;
            }
            
            _playableDirector.time = 0.0f;
            _isPlaying = true;
        }

        /// <summary>
        /// 停止処理
        /// </summary>
        void ICutscene.Stop() {
            if (_playableDirector == null) {
                return;
            }
            
            _isPlaying = false;
            _playableDirector.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void ICutscene.Update(float deltaTime) {
            if (_playableDirector == null) {
                return;
            }
            
            var time = _playableDirector.time + deltaTime;
            if (time >= _playableDirector.duration &&
                (_playableDirector.extrapolationMode == DirectorWrapMode.Hold || _playableDirector.extrapolationMode == DirectorWrapMode.None)) {
                time = _playableDirector.duration;
                _isPlaying = false;
            }

            _playableDirector.time = time;
            _playableDirector.Evaluate();
        }

        /// <summary>
        /// オブジェクトのバインド
        /// </summary>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        public void Bind(string trackName, Object target) {
            if (_playableDirector == null) {
                return;
            }
            
            var binding = _playableDirector.playableAsset.outputs.FirstOrDefault(x => x.streamName == trackName);
            if (binding.sourceObject == null) {
                Debug.unityLogger.LogWarning(_playableDirector.name, $"Not found bind trackName. [{trackName}]");
                return;
            }
            
            _playableDirector.SetGenericBinding(binding.sourceObject, target);
            _bindingTrackKeys.Add(binding.sourceObject);
        }
    }
}