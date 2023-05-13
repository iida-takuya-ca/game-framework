using Cinemachine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラを操作するためのComponent
    /// </summary>
    public interface ICameraComponent {
        // アクティブ状態
        bool IsActive { get; }

        // 基本になるCinemachineCamera
        ICinemachineCamera BaseCamera { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();

        /// <summary>
        /// アクティブ化
        /// </summary>
        void Activate();

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update(float deltaTime);
    }
}