namespace GameFramework.TaskSystems {
    /// <summary>
    /// タスク用インターフェース
    /// </summary>
    public interface ITask {
        // タスクの有効状態
        bool IsActive { get; }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();
    }

    /// <summary>
    /// 後更新可能なタスク用インターフェース
    /// </summary>
    public interface ILateUpdatableTask : ITask {
        /// <summary>
        /// 後更新処理
        /// </summary>
        void LateUpdate();
    }

    /// <summary>
    /// タスクイベント通知用インターフェース
    /// </summary>
    public interface ITaskEventHandler {
        /// <summary>
        /// 登録時処理
        /// </summary>
        void OnRegistered(TaskRunner runner);

        /// <summary>
        /// 登録解除時処理
        /// </summary>
        void OnUnregistered(TaskRunner runner);
    }
}