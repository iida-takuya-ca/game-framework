using System;
using UnityEngine;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// タスク用インターフェース
    /// </summary>
    public class TaskAgent : ILateUpdatableTask, ITaskEventHandler, IDisposable {
        // 登録されているTaskManager
        private TaskRunner _owner;
        
        // 更新通知
        public event Action OnUpdate;
        // 後更新通知
        public event Action OnLateUpdate;
        
        // Taskが有効か
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_owner != null) {
                _owner.Unregister(this);
            }
        }
        
        /// <summary>
        /// タスク更新
        /// </summary>
        void ITask.Update() {
            OnUpdate?.Invoke();
        }

        /// <summary>
        /// タスク後更新
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            OnLateUpdate?.Invoke();            
        }

        /// <summary>
        /// タスク登録通知
        /// </summary>
        void ITaskEventHandler.OnRegistered(TaskRunner taskRunner) {
            if (_owner != null) {
                Debug.LogWarning("Double registered task.");
                _owner.Unregister(this);
            }
            _owner = taskRunner;
        }

        /// <summary>
        /// タスク登録解除通知
        /// </summary>
        void ITaskEventHandler.OnUnregistered(TaskRunner taskRunner) {
            if (taskRunner == _owner) {
                _owner = null;
            }
        }
    }
}