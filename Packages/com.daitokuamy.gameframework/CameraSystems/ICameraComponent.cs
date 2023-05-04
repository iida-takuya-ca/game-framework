namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラを操作するためのComponent
    /// </summary>
    public interface ICameraComponent {
        // アクティブ状態
        bool IsActive { get; }
        
        /// <summary>
        /// アクティブ化
        /// </summary>
        void Activate();
        
        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void Deactivate();
    }
}