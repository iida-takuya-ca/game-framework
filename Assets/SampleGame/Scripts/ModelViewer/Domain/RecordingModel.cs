using GameFramework.ModelSystems;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 録画用モデル
    /// </summary>
    public class RecordingModel : AutoIdModel<RecordingModel> {
        /// <summary>録画モードマスク</summary>
        public RecordingModeFlags ModeFlags { get; private set; }
        
        /// <summary>
        /// 録画モードの変更
        /// </summary>
        public void SetModeFlags(RecordingModeFlags recordingModeFlags) {
            ModeFlags = recordingModeFlags;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private RecordingModel(int id) 
            : base(id) {}
    }
}
