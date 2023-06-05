using GameFramework.ModelSystems;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 録画用モデル
    /// </summary>
    public class RecordingModel : AutoIdModel<RecordingModel> {
        /// <summary>録画モードマスク</summary>
        public RecordingModeFlags ModeFlags { get; private set; }
        /// <summary>回転時間</summary>
        public float RotationDuration { get; private set; } = 2.0f;
        
        /// <summary>
        /// 録画モードの変更
        /// </summary>
        public void SetModeFlags(RecordingModeFlags recordingModeFlags) {
            ModeFlags = recordingModeFlags;
        }

        /// <summary>
        /// 回転時間の設定
        /// </summary>
        public void SetRotationDuration(float duration) {
            RotationDuration = Mathf.Max(0.1f, duration);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private RecordingModel(int id) 
            : base(id) {}
    }
}
