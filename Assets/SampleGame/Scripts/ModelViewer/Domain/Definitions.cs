using System;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// カメラの操作タイプ
    /// </summary>
    public enum CameraControlType {
        Default,
        SceneView,
    }

    /// <summary>
    /// 録画モードマスク
    /// </summary>
    [Flags]
    public enum RecordingModeFlags {
        CharacterRotation = 1 << 0,
        CameraRotation = 1 << 1,
        LightRotation = 1 << 2,
    }
}
