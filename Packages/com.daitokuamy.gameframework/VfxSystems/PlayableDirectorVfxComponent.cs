using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// PlayableDirector制御用のVfxComponent
    /// </summary>
    public class PlayableDirectorVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("再生に使うPlayableDirector")]
        private PlayableDirector _playableDirector;

        // 再生中か
        bool IVfxComponent.IsPlaying => _playableDirector.state == PlayState.Playing;
        // 有効なデータか
        private bool IsValid => _playableDirector != null;

        /// <summary>
        /// 更新処理
        /// </summary>
        void IVfxComponent.Update(float deltaTime) {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time += deltaTime;
            _playableDirector.Evaluate();
        }

        /// <summary>
        /// 再生
        /// </summary>
        void IVfxComponent.Play() {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time = 0.0f;
        }

        /// <summary>
        /// 停止
        /// </summary>
        void IVfxComponent.Stop() {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time = _playableDirector.duration;
            _playableDirector.Evaluate();
            _playableDirector.Stop();
        }

        /// <summary>
        /// 即時停止
        /// </summary>
        void IVfxComponent.StopImmediate() {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time = _playableDirector.duration;
            _playableDirector.Evaluate();
            _playableDirector.Stop();
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void IVfxComponent.SetSpeed(float speed) {
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            if (_playableDirector == null) {
                Debug.LogWarning($"Invalid serialize data. : {gameObject.name}");
                return;
            }

            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
            _playableDirector.playOnAwake = false;
            _playableDirector.Stop();
        }
    }
}