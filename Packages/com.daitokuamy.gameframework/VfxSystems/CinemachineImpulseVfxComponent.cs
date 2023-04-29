#if USE_CINEMACHINE

using Cinemachine;
using UnityEngine;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// CinemachineImpulse制御用のVfxComponent
    /// </summary>
    public class CinemachineImpulseVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("衝撃設定")]
        private CinemachineImpulseSource _impulseSource;

        // 再生速度
        private float _speed = 1.0f;
        // デフォルトの再生時間
        private float _defaultDuration;
        // 再生中タイマー
        private float _timer = 0.0f;

        // 再生中か
        bool IVfxComponent.IsPlaying => _timer > float.Epsilon;

        /// <summary>
        /// 更新処理
        /// </summary>
        void IVfxComponent.Update(float deltaTime) {
            _timer -= deltaTime;
        }
        
        /// <summary>
        /// 再生
        /// </summary>
        void IVfxComponent.Play() {
            if (_impulseSource == null) {
                return;
            }
            
            _impulseSource.enabled = true;
            _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = _defaultDuration / _speed;
            _impulseSource.GenerateImpulse();
            _timer = _impulseSource.m_ImpulseDefinition.m_ImpulseDuration;
        }

        /// <summary>
        /// 停止
        /// </summary>
        void IVfxComponent.Stop() {
            _impulseSource.enabled = false;
            _timer = 0.0f;
        }

        /// <summary>
        /// 即時停止
        /// </summary>
        void IVfxComponent.StopImmediate() {
            _impulseSource.enabled = false;
            _timer = 0.0f;
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void IVfxComponent.SetSpeed(float speed) {
            _speed = Mathf.Max(0.001f, speed);
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _defaultDuration = _impulseSource.m_ImpulseDefinition.m_ImpulseDuration;
        }
    }
}

#endif