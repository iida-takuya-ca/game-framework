using GameFramework.CameraSystems;
using UnityEngine.InputSystem;

namespace SampleGame.Viewer {
    /// <summary>
    /// モデルビューア用カメラ制御クラス
    /// </summary>
    public class ModelViewerCameraController : CameraController<PreviewCameraComponent> {
        private float _mouseLeftDeltaSpeed = 1.0f;
        private float _mouseMiddleDeltaSpeed = 0.005f;
        private float _mouseRightDeltaSpeed = 0.01f;
        private float _mouseScrollSpeed = 0.005f;
        
        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            // マウス移動による移動
            if (Mouse.current.leftButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _mouseLeftDeltaSpeed;
                Component.AngleY += delta.x;
                Component.AngleX -= delta.y;
            }

            if (Mouse.current.rightButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _mouseRightDeltaSpeed;
                Component.Distance -= delta.x + delta.y;
            }

            if (Mouse.current.middleButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _mouseMiddleDeltaSpeed;
                var lookAtOffset = Component.LookAtOffset;
                lookAtOffset.x -= delta.x;
                lookAtOffset.y -= delta.y;
                Component.LookAtOffset = lookAtOffset;
            }

            var scroll = Mouse.current.scroll.ReadValue() * _mouseScrollSpeed;
            Component.Distance -= scroll.y;
        }
    }
}
