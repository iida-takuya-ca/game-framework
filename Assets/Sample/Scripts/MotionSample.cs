using System;
using GameFramework.BodySystems;
using GameFramework.Core;
using UnityEngine;
using GameFramework.TaskSystems;
using UnityEngine.Serialization;

/// <summary>
/// モーション制御サンプル
/// </summary>
public class MotionSample : MonoBehaviour {
    private enum MotionState {
        None,
        AnimatorController,
        AnimationClip,
    }
    
    [SerializeField, Tooltip("再生するAnimatorController")]
    private RuntimeAnimatorController _controller;
    [SerializeField, Tooltip("再生するAnimationClip")]
    private AnimationClip _clip;
    [SerializeField, Tooltip("AnimatorControllerとAnimationClipのブレンド時間")]
    private float _blendTime = 0.3f;
    [SerializeField, Tooltip("表示に使うBodyのPrefab")]
    private GameObject _bodyTarget;
    [SerializeField, Tooltip("モーション速度")]
    private float _timeScale = 1.0f;

    private ServiceContainer _serviceContainer;
    private TaskRunner _taskRunner;
    private BodyManager _bodyManager;

    private Body _body;
    private MotionState _motionState;

    /// <summary>
    /// 生成時処理
    /// </summary>
    private void Awake() {
        _serviceContainer = new ServiceContainer();
        
        _taskRunner = new TaskRunner();
        _serviceContainer.Set(_serviceContainer);
        _bodyManager = new BodyManager();
        _serviceContainer.Set(_bodyManager);
        _taskRunner.Register(_bodyManager);
    }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        _body = _bodyManager.CreateFromGameObject(_bodyTarget);
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update() {
        // モーション切り替え
        if (Input.GetKeyDown(KeyCode.Space)) {
            _motionState = (MotionState)((int)(_motionState + 1) % Enum.GetValues(typeof(MotionState)).Length);
            var motionController = _body.GetController<MotionController>();
            switch (_motionState) {
                case MotionState.None:
                    motionController.ResetMotion(_blendTime);
                    break;
                case MotionState.AnimationClip:
                    motionController.SetMotion(_clip, _blendTime);
                    break;
                case MotionState.AnimatorController:
                    motionController.SetMotion(_controller, _blendTime);
                    break;
            }
        }

        // 更新速度変更
        _body.LayeredTime.LocalTimeScale = _timeScale;
        
        _taskRunner.Update();
    }

    private void LateUpdate() {
        _taskRunner.LateUpdate();
    }

    /// <summary>
    /// 廃棄時処理
    /// </summary>
    private void OnDestroy() {
        _serviceContainer.Dispose();
    }
}