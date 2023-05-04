using System;
using System.Collections.Generic;
using Cinemachine;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラ管理クラス
    /// </summary>
    public class CameraManager : LateUpdatableTaskBehaviour, IDisposable {
        /// <summary>
        /// カメラハンドリング用クラス
        /// </summary>
        private class CameraHandler : IDisposable {
            private int _activateCount;
            
            public string Name { get; }
            public ICameraComponent Component { get; }
            public ICameraController Controller { get; private set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CameraHandler(string cameraName, ICameraComponent cameraComponent) {
                Name = cameraName;
                Component = cameraComponent;
                ApplyActiveStatus();
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                ChangeController(null);
            }

            /// <summary>
            /// アクティブ化
            /// </summary>
            public void Activate() {
                _activateCount++;
                
                ApplyActiveStatus();
            }

            /// <summary>
            /// 非アクティブ化
            /// </summary>
            public void Deactivate() {
                _activateCount--;

                if (_activateCount < 0) {
                    Debug.LogWarning($"activateCount is minus. [{Name}]");
                }
                
                ApplyActiveStatus();
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public void Update(float deltaTime) {
                if (Controller != null) {
                    Controller.Update(deltaTime);
                }
            }

            /// <summary>
            /// コントローラーの変更
            /// </summary>
            public void ChangeController(ICameraController controller) {
                if (Controller != null) {
                    Controller.Dispose();
                    Controller = null;
                }

                Controller = controller;
                if (Controller != null) {
                    Controller.Initialize(Component);
                    if (Component.IsActive) {
                        controller.Activate();
                    }
                }
            }

            /// <summary>
            /// アクティブ状態の反映
            /// </summary>
            private void ApplyActiveStatus() {
                var active = _activateCount > 0;
                if (active != Component.IsActive) {
                    if (active) {
                        Component.Activate();
                        Controller?.Activate();
                    }
                    else {
                        Component.Deactivate();
                        Controller?.Deactivate();
                    }
                }
            }
        }

        [SerializeField, Tooltip("仮想カメラ用のBrain")]
        private CinemachineBrain _brain;
        [SerializeField, Tooltip("仮想カメラを配置しているRootObject")]
        private GameObject _virtualCameraRoot;
        [SerializeField, Tooltip("仮想カメラの基準Transformを配置しているRootObject")]
        private GameObject _targetPointRoot;
        [SerializeField, Tooltip("デフォルトで使用するカメラ名")]
        private string _defaultCameraName = "Default";

        // 初期化済みフラグ
        private bool _initialized;
        // 廃棄済みフラグ
        private bool _disposed;
        // Brainの初期状態のUpdateMethod
        private CinemachineBrain.UpdateMethod _defaultUpdateMethod;
        // カメラハンドリング用情報
        private Dictionary<string, CameraHandler> _cameraHandlers = new();
        // 基準Transform
        private Dictionary<string, Transform> _targetPoints = new();

        // 出力先のカメラ
        public Camera OutputCamera => _brain != null ? _brain.OutputCamera : null;
        // LayeredTime
        public LayeredTime LayeredTime { get; private set; } = new LayeredTime();

        /// <summary>
        /// カメラのアクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        public void Activate(string cameraName) {
            Initialize();
            
            if (!_cameraHandlers.TryGetValue(cameraName, out var handler)) {
                return;
            }

            handler.Activate();
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        public void Deactivate(string cameraName) {
            Initialize();
            
            if (!_cameraHandlers.TryGetValue(cameraName, out var handler)) {
                return;
            }

            handler.Deactivate();
        }

        /// <summary>
        /// CameraComponentの取得
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraComponent GetCameraComponent<TCameraComponent>(string cameraName)
            where TCameraComponent : class, ICameraComponent {
            Initialize();
            
            if (!_cameraHandlers.TryGetValue(cameraName, out var handler)) {
                return default;
            }

            return handler.Component as TCameraComponent;
        }

        /// <summary>
        /// CameraControllerの切り替え
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        /// <param name="cameraController">設定するController</param>
        public void ChangeCameraController(string cameraName, ICameraController cameraController) {
            Initialize();
            
            if (!_cameraHandlers.TryGetValue(cameraName, out var handler)) {
                return;
            }
            
            handler.ChangeController(cameraController);
        }

        /// <summary>
        /// ターゲットポイント(仮想カメラが参照するTransform)の取得
        /// </summary>
        /// <param name="targetPointName">ターゲットポイント名</param>
        public Transform GetTargetPoint(string targetPointName) {
            Initialize();

            if (!_targetPoints.TryGetValue(targetPointName, out var targetPoint)) {
                return null;
            }

            return targetPoint;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            
            // Cameraの設定を戻す
            _brain.m_UpdateMethod = _defaultUpdateMethod; 
            
            // カメラ情報を廃棄
            foreach (var pair in _cameraHandlers) {
                pair.Value.Dispose();
            }
            
            _cameraHandlers.Clear();
            _targetPoints.Clear();
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void AwakeInternal() {
            Initialize();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void OnDestroyInternal() {
            Dispose();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = LayeredTime.DeltaTime;
            
            // Controllerの更新
            foreach (var pair in _cameraHandlers) {
                pair.Value.Update(deltaTime);
            }
            
            // Brainの更新
            CinemachineCore.UniformDeltaTimeOverride = deltaTime;
            _brain.ManualUpdate();
        }

        /// <summary>
        /// カメラ制御用ハンドラーの生成
        /// </summary>
        private void CreateCameraHandlers() {
            _cameraHandlers.Clear();

            if (_virtualCameraRoot == null) {
                Debug.LogWarning("Not found virtual camera root.");
                return;
            }
            
            // 1階層下の仮想カメラを元にカメラ情報を構築
            foreach (Transform child in _virtualCameraRoot.transform) {
                var vcam = child.GetComponent<CinemachineVirtualCameraBase>();
                if (vcam == null) {
                    continue;
                }

                // カメラ名が既にあれば何もしない
                var cameraName = vcam.name;
                if (_cameraHandlers.ContainsKey(cameraName)) {
                    Debug.LogWarning($"Already exists camera name. [{cameraName}]");
                    continue;
                }

                // Camera制御用Componentを取得(無ければDefaultの物を使用)
                var component = child.GetComponent<ICameraComponent>();
                if (component == null) {
                    component = new DefaultCameraComponent(vcam);
                }

                var handler = new CameraHandler(cameraName, component);
                _cameraHandlers[cameraName] = handler;
            }
        }

        /// <summary>
        /// TargetPoint情報の生成
        /// </summary>
        private void CreateTargetPoints() {
            _targetPoints.Clear();
            
            foreach (Transform targetPoint in _targetPointRoot.transform) {
                var targetPointName = targetPoint.name;
                if (_targetPoints.ContainsKey(targetPointName)) {
                    targetPoint.gameObject.SetActive(false);
                    continue;
                }
                
                _targetPoints[targetPointName] = targetPoint;
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;

            _defaultUpdateMethod = _brain.m_UpdateMethod;
            _brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
            CreateCameraHandlers();
            CreateTargetPoints();
            
            // デフォルトのカメラをActivate
            Activate(_defaultCameraName);
        }
    }
}