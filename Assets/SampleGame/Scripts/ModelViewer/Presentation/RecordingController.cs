using System;
using System.Collections;
using GameFramework.CameraSystems;
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
            _coroutineRunner.StartCoroutine(RecordRoutine(model.RotationDuration, model.ModeFlags), () => { op.Completed(); }, () => { op.Canceled(); }, ex => { op.Canceled(ex); });
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
        private IEnumerator RecordRoutine(float rotationDuration, RecordingModeFlags flags) {
            var slot = Services.Get<ModelViewerSettings>().PreviewSlot;
            var cameraComponent = Services.Get<CameraManager>().GetCameraComponent<PreviewCameraComponent>("Default");
            var dirLight = Services.Get<EnvironmentManager>().CurrentLight;

            void SetSlotAngle(float angle) {
                if (slot == null) {
                    return;
                }

                var slotEulerAngles = slot.eulerAngles;
                slotEulerAngles.y = angle;
                slot.eulerAngles = slotEulerAngles;
            }

            void SetCameraAngle(float angle) {
                if (cameraComponent == null) {
                    return;
                }

                cameraComponent.AngleY = 0.0f;
            }

            void SetLightAngle(float angle) {
                if (dirLight == null) {
                    return;
                }

                var lightTrans = dirLight.transform;
                var lightEulerAngles = lightTrans.eulerAngles;
                lightEulerAngles.y = 0.0f;
                lightTrans.eulerAngles = lightEulerAngles;
            }

            // 各種向きをリセット
            SetSlotAngle(0.0f);
            SetCameraAngle(0.0f);
            SetLightAngle(0.0f);
            
            // todo:録画開始

            IEnumerator RotateRoutine(Action<float> setAngleAction) {
                var time = 0.0f;
                while (true) {
                    // 回転
                    var angle = 360.0f * Mathf.Clamp01(time / rotationDuration);
                    setAngleAction(angle);
                    if (time >= rotationDuration) {
                        break;
                    }

                    time += Time.deltaTime;
                    yield return null;
                }
            }
            
            // 回転の実行
            if ((flags & RecordingModeFlags.ActorRotation) != 0) {
                yield return RotateRoutine(SetSlotAngle);
            }
            if ((flags & RecordingModeFlags.CameraRotation) != 0) {
                yield return RotateRoutine(SetCameraAngle);
            }
            if ((flags & RecordingModeFlags.LightRotation) != 0) {
                yield return RotateRoutine(SetLightAngle);
            }
            
            // todo:録画停止
        }
    }
}