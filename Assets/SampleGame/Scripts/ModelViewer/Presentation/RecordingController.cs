using System.Collections;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using GameFramework.TaskSystems;
using UnityEditor.Recorder;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 録画するためのコントローラ
    /// </summary>
    public class RecordingController : TaskBehaviour {
        private MovieRecorderSettings _settings;
        private CoroutineRunner _coroutineRunner;
        
        /// <summary>
        /// 録画処理
        /// </summary>
        public AsyncOperationHandle RecordAsync() {
            var op = new AsyncOperator();
            var model = ModelViewerModel.Get().RecordingModel;
            _coroutineRunner.StartCoroutine(RecordRoutine(model.ModeFlags), () => { op.Completed(); }, () => { op.Canceled(); }, ex => { op.Canceled(ex); });
            return op;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void AwakeInternal() {
            _settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            _coroutineRunner = new CoroutineRunner();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void OnDestroyInternal() {
            _coroutineRunner.Dispose();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 録画ルーチン
        /// </summary>
        private IEnumerator RecordRoutine(RecordingModeFlags flags) {
            yield break;
        }
    }
}