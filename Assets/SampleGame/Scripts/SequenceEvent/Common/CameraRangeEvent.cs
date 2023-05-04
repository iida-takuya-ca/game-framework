using ActionSequencer;
using GameFramework.CameraSystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Camera切り替え再生イベント
    /// </summary>
    public class CameraRangeEvent : RangeSequenceEvent {
        [Tooltip("切り替えるカメラ名")]
        public string cameraName = "";
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class CameraRangeEventHandler : RangeSequenceEventHandler<CameraRangeEvent> {
        private CameraManager _cameraManager;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(CameraManager manager) {
            _cameraManager = manager;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(CameraRangeEvent sequenceEvent) {
            if (_cameraManager != null) {
                _cameraManager.Activate(sequenceEvent.cameraName);
            }
        }

        /// <summary>
        /// 終了時処理
        /// </summary>
        protected override void OnExit(CameraRangeEvent sequenceEvent) {
            if (_cameraManager != null) {
                _cameraManager.Deactivate(sequenceEvent.cameraName);
            }
        }

        /// <summary>
        /// キャンセル時処理
        /// </summary>
        protected override void OnCancel(CameraRangeEvent sequenceEvent) {
            OnExit(sequenceEvent);
        }
    }
}
