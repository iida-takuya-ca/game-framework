using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace GameFramework.CutsceneSystems {
    /// <summary>
    /// カットシーンクラス
    /// </summary>
    public class Cutscene : MonoBehaviour, ICutscene, INotificationReceiver {
        private PlayableDirector _playableDirector;
        private bool _isPlaying;
        private DisposableScope _scope;

        private List<Object> _bindingTrackKeys = new();

        /// <summary>再生中か</summary>
        bool ICutscene.IsPlaying => _isPlaying;

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICutscene.Initialize() {
            _playableDirector = GetComponent<PlayableDirector>();
            
            if (_playableDirector == null) {
                return;
            }
            
            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;

            _scope = new DisposableScope();
            InitializeInternal(_scope);
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
            
            DisposeInternal();
            _scope.Dispose();
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
            PlayInternal();
        }

        /// <summary>
        /// 停止処理
        /// </summary>
        void ICutscene.Stop() {
            if (_playableDirector == null) {
                return;
            }
            
            _isPlaying = false;
            gameObject.SetActive(false);
            StopInternal();
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
            
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 通知の受信
        /// </summary>
        /// <param name="origin">発生元のPlayable</param>
        /// <param name="notification">通知内容</param>
        /// <param name="context">通知用ユーザー定義データ</param>
        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context) {
            OnNotifyInternal(origin, notification, context);
        }

        /// <summary>
        /// オブジェクトのバインド
        /// </summary>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        protected void Bind(string trackName, Object target) {
            if (_playableDirector == null) {
                return;
            }
            
            var binding = _playableDirector.playableAsset.outputs.FirstOrDefault(x => x.streamName == trackName);
            if (binding.sourceObject == null) {
                Debug.unityLogger.LogWarning(name, $"Not found bind trackName. [{trackName}]");
                return;
            }
            
            _playableDirector.SetGenericBinding(binding.sourceObject, target);
            _bindingTrackKeys.Add(binding.sourceObject);
        }
        
        /// <summary>
        /// 初期化時処理(Override用)
        /// </summary>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <summary>
        /// 破棄時処理(Override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 再生時処理(Override用)
        /// </summary>
        protected virtual void PlayInternal() {
        }

        /// <summary>
        /// 停止時処理(Override用)
        /// </summary>
        protected virtual void StopInternal() {
        }

        /// <summary>
        /// 更新時処理(Override用)
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 再生速度の設定(Override用)
        /// </summary>
        /// <param name="timeScale">再生速度</param>
        protected virtual void SetSpeedInternal(float timeScale) {
        }

        /// <summary>
        /// Markerの受信処理(Override用)
        /// </summary>
        /// <param name="origin">発生元のPlayable</param>
        /// <param name="notification">通知内容</param>
        /// <param name="context">通知用ユーザー定義データ</param>
        protected virtual void OnNotifyInternal(Playable origin, INotification notification, object context) {
        }
    }
}