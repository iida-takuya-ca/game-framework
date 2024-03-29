using System.Collections;
using GameFramework.CollisionSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体の実体制御用MonoBehaviour
    /// </summary>
    public class ParticleProjectileObject : ProjectileObject {
        [SerializeField, Tooltip("飛翔中に再生するParticleSystem(Loop)")]
        private ParticleSystem _baseParticle;
        [SerializeField, Tooltip("ヒット時再生するParticleSystem(OneShot)")]
        private ParticleSystem _hitParticle;
        [SerializeField, Tooltip("終了時再生するParticleSystem(OneShot)")]
        private ParticleSystem _exitParticle;

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected override void OnStartProjectile() {
            StopParticle(_hitParticle);
            StopParticle(_exitParticle);

            PlayParticle(_baseParticle);
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected override IEnumerator ExitProjectileRoutine() {
            StopParticle(_baseParticle);
            PlayParticle(_exitParticle);

            // Particleの再生が完了するまで待つ
            while (true) {
                if (IsAliveParticle(_baseParticle) || IsAliveParticle(_hitParticle) || IsAliveParticle(_exitParticle)) {
                    yield return null;
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        protected override void OnHitCollision(RaycastHitResult result) {
            PlayParticle(_hitParticle);
        }

        /// <summary>
        /// Particleを再生
        /// </summary>
        private void PlayParticle(ParticleSystem particle) {
            if (particle == null) {
                return;
            }

            particle.Play();
        }

        /// <summary>
        /// Particleを停止
        /// </summary>
        private void StopParticle(ParticleSystem particle) {
            if (particle == null) {
                return;
            }

            particle.Stop();
        }

        /// <summary>
        /// Particleが生存中か
        /// </summary>
        private bool IsAliveParticle(ParticleSystem particle) {
            if (particle == null) {
                return false;
            }

            return particle.IsAlive(true);
        }
    }
}