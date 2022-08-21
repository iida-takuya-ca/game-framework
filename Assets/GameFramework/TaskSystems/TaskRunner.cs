using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// タスク実行用のランナー
    /// </summary>
    public class TaskRunner : ILateUpdatableTask, IDisposable {
        // タスクの状態
        private enum TaskStatus {
            Scheduled,
            Active,
            Killed,
        }

        // タスク情報
        private class TaskInfo {
            public TaskGroupInfo groupInfo;
            public TaskStatus status;
            public ITask task;
            public CustomSampler[] samplers;
        }

        // タスクグループ情報
        private class TaskGroupInfo {
            public List<TaskInfo> taskInfos = new List<TaskInfo>();
            public CustomSampler[] samplers;
        }

        // Order毎のタスクグループ情報
        private IDictionary<int, TaskGroupInfo> _taskGroupInfos = new SortedDictionary<int, TaskGroupInfo>();
        // TaskInfo検索用
        private Dictionary<ITask, TaskInfo> _taskInfos = new Dictionary<ITask, TaskInfo>();
        
        // タスクの有効状態
        public bool IsActive { get; set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            foreach (var groupInfo in _taskGroupInfos.Values) {
                for (var i = groupInfo.taskInfos.Count - 1; i >= 0; i--) {
                    var taskInfo = groupInfo.taskInfos[i];
                    if (taskInfo.status != TaskStatus.Killed) {
                        taskInfo.status = TaskStatus.Killed;
                        
                        // 登録解除通知
                        if (taskInfo.task is ITaskEventHandler handler) {
                            handler.OnUnregistered(this);
                        }
                    }
                }
            }
            
            // 各種リストリセット
            _taskGroupInfos.Clear();
            _taskInfos.Clear();
        }
        
        /// <summary>
        /// タスク登録
        /// </summary>
        /// <param name="task">追加対象のTask</param>
        /// <param name="executionOrder">Taskの実行優先度</param>
        public void Register(ITask task, int executionOrder = 0) {
            if (task == null) {
                Debug.LogError("Registered task is null.");
                return;
            }

            // TaskGroupInfoの取得/生成
            if (!_taskGroupInfos.TryGetValue(executionOrder, out var groupInfo)) {
                groupInfo = new TaskGroupInfo();
                groupInfo.samplers = new[] {
                    CustomSampler.Create($"Task Update[{executionOrder}]"),
                    CustomSampler.Create($"Task LateUpdate[{executionOrder}]"),
                };
                _taskGroupInfos[executionOrder] = groupInfo;
            }
            
            // 既に存在しているTaskの場合、ステータスを更新
            if (_taskInfos.TryGetValue(task, out var info)) {
                // Groupが変わっていたら変更
                if (info.groupInfo != groupInfo) {
                    info.groupInfo.taskInfos.Remove(info);
                    groupInfo.taskInfos.Add(info);
                    info.groupInfo = groupInfo;
                }
                
                // Killされていた場合、Scheduleに戻す
                if (info.status == TaskStatus.Killed) {
                    info.status = TaskStatus.Scheduled;

                    // 登録通知
                    if (task is ITaskEventHandler handler) {
                        handler.OnRegistered(this);
                    }
                }
            }
            // 存在していない場合、タスク情報を追加
            else {
                var taskType = task.GetType();
                info = new TaskInfo {
                    status = TaskStatus.Scheduled,
                    task = task,
                    samplers = new[] {
                        CustomSampler.Create($"{taskType}.Update"),
                        CustomSampler.Create($"{taskType}.LateUpdate")
                    }
                };
                groupInfo.taskInfos.Add(info);
                _taskInfos[task] = info;
                
                // 登録通知
                if (task is ITaskEventHandler handler) {
                    handler.OnRegistered(this);
                }
            }
        }

        /// <summary>
        /// タスク登録
        /// </summary>
        /// <param name="task">追加対象のTask</param>
        /// <param name="executionOrder">Taskの実行優先度</param>
        /// <typeparam name="T">実行優先度を指定するenum型</typeparam>
        public void Register<T>(ITask task, T executionOrder)
        where T : Enum {
            Register(task, Convert.ToInt32(executionOrder));
        }

        /// <summary>
        /// タスク登録解除
        /// </summary>
        /// <param name="task">除外対象のTask</param>
        public void Unregister(ITask task) {
            if (!_taskInfos.TryGetValue(task, out var info)) {
                return;
            }
            
            if (info.status != TaskStatus.Killed) {
                // タスクのステータスをKilledに変更
                info.status = TaskStatus.Killed;
                
                // 除外通知
                if (task is ITaskEventHandler handler) {
                    handler.OnUnregistered(this);
                }
            }
        }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            UpdateInternal(task => task.Update(), 0);
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            UpdateInternal(task => {
                if (task is ILateUpdatableTask lateUpdatableTask) {
                    lateUpdatableTask.LateUpdate();
                }
            }, 1);
        }

        /// <summary>
        /// 内部用更新処理
        /// </summary>
        private void UpdateInternal([NotNull]Action<ITask> onUpdate, int samplerIndex) {
            // Taskリストのリフレッシュ
            RefreshTaskInfos();
            
            foreach (var groupInfo in _taskGroupInfos.Values) {
                groupInfo.samplers[samplerIndex].Begin();

                for (var i = 0; i < groupInfo.taskInfos.Count; i++) {
                    var taskInfo = groupInfo.taskInfos[i];

                    // 無効なタスクは処理しない
                    if (taskInfo.status != TaskStatus.Active || !taskInfo.task.IsActive) {
                        continue;
                    }
                    
                    // タスク更新
                    try {
                        taskInfo.samplers[samplerIndex].Begin();
                        onUpdate(taskInfo.task);
                    }
                    catch (Exception exception) {
                        Debug.LogException(exception);
                    }
                    finally {
                        taskInfo.samplers[samplerIndex].End();
                    }
                }
                
                groupInfo.samplers[samplerIndex].End();
            }
        }

        /// <summary>
        /// タスク情報のリフレッシュ
        /// </summary>
        private void RefreshTaskInfos() {
            foreach (var groupInfo in _taskGroupInfos.Values) {
                for (var i = groupInfo.taskInfos.Count - 1; i >= 0; i--) {
                    var taskInfo = groupInfo.taskInfos[i];

                    // KillされたTaskは除外
                    if (taskInfo.status == TaskStatus.Killed) {
                        groupInfo.taskInfos.RemoveAt(i);
                        _taskInfos.Remove(taskInfo.task);
                    }
                    // ScheduleされたTaskは有効化
                    else if (taskInfo.status == TaskStatus.Scheduled) {
                        taskInfo.status = TaskStatus.Active;
                    }
                }
            }
        }
    }
}