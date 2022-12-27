using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using GameFramework.Kinematics;
using GameFramework.TaskSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// カメラ制御クラス
    /// </summary>
    public class CameraController : MonoBehaviour, ILateUpdatableTask {
        [SerializeField, Tooltip("メインカメラ")]
        private Camera _camera;
        [SerializeField, Tooltip("VirtualCameraのRoot")]
        private GameObject _virtualCameraRoot;
        [SerializeField, Tooltip("AttachmentのRoot")]
        private GameObject _attachmentRoot;

        private CinemachineBrain _brain;
        private CinemachineVirtualCameraBase[] _virtualCameras;
        private Attachment[] _attachments;

        public bool IsActive => isActiveAndEnabled;
        public Camera MainCamera => _camera;

        /// <summary>
        /// カメラのアクティブ化
        /// </summary>
        public void Activate(string cameraName) {
            var virtualCamera = _virtualCameras.FirstOrDefault(x => x.name == cameraName);
            if (virtualCamera == null) {
                return;
            }

            virtualCamera.gameObject.SetActive(true);
        }

        /// <summary>
        /// カメラの非アクティブ化
        /// </summary>
        public void Deactivate(string cameraName) {
            var virtualCamera = _virtualCameras.FirstOrDefault(x => x.name == cameraName);
            if (virtualCamera == null) {
                return;
            }

            virtualCamera.gameObject.SetActive(false);
        }

        /// <summary>
        /// Attachmentの取得
        /// </summary>
        public T GetAttachment<T>(string attachmentName)
            where T : Attachment {
            var attachment = _attachments.FirstOrDefault(x => x.name == attachmentName);
            return attachment as T;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ITask.Update() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            // Attachment更新
            foreach (var attachment in _attachments) {
                attachment.ManualUpdate();
            }

            // Brain更新
            _brain.ManualUpdate();
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _brain = _camera.GetComponent<CinemachineBrain>();
            _brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;

            var virtualCameraList = new List<CinemachineVirtualCameraBase>();
            for (var i = 0; i < _virtualCameraRoot.transform.childCount; i++) {
                var virtualCamera = _virtualCameraRoot.transform.GetChild(i)
                    .GetComponent<CinemachineVirtualCameraBase>();
                if (virtualCamera == null) {
                    continue;
                }

                virtualCameraList.Add(virtualCamera);
            }

            _virtualCameras = virtualCameraList.ToArray();
            _attachments = _attachmentRoot.GetComponentsInChildren<Attachment>();
            foreach (var attachment in _attachments) { 
                attachment.UpdateMode = Attachment.Mode.Manual;
            }
        }
    }
}