using System;
using System.Collections;
using GameFramework.CollisionSystems;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体オブジェクト用インターフェース
    /// </summary>
    public interface IProjectileObject : IDisposable {
        /// <summary>
        /// 再生中か
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// レイキャスト用の半径（0より大きいとSphereCast）
        /// </summary>
        float RaycastRadius { get; }

        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        /// <param name="active">アクティブか</param>
        void SetActive(bool active);

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        void StartProjectile(IProjectile projectile);

        /// <summary>
        /// 飛翔更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void UpdateProjectile(float deltaTime);

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void ExitProjectile();

        /// <summary>
        /// 座標の更新
        /// </summary>
        /// <param name="position">更新後の座標</param>
        /// <param name="rotation">更新後の姿勢</param>
        void UpdateTransform(Vector3 position, Quaternion rotation);

        /// <summary>
        /// 衝突発生通知
        /// </summary>
        /// <param name="result">衝突結果</param>
        void OnHitCollision(RaycastHitResult result);
    }

    /// <summary>
    /// 飛翔体の実体制御用MonoBehaviour
    /// </summary>
    public class ProjectileObject : MonoBehaviour, IProjectileObject {
        [SerializeField, Tooltip("レイキャスト用の半径(0より大きいとSphereRaycast)")]
        private float _raycastRadius = 0.0f;

        private bool _isPlaying;
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();

        // 再生中か
        bool IProjectileObject.IsPlaying => _isPlaying;
        // レイキャスト用の半径（0より大きいとSphereCast）
        float IProjectileObject.RaycastRadius => _raycastRadius;
        // 使用中のProjectile
        protected IProjectile Projectile { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (gameObject == null) {
                return;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        void IProjectileObject.SetActive(bool active) {
            if (gameObject == null) {
                return;
            }
            
            if (gameObject.activeSelf == active) {
                return;
            }

            gameObject.SetActive(active);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        void IProjectileObject.StartProjectile(IProjectile projectile) {
            if (_isPlaying) {
                return;
            }

            Projectile = projectile;
            ((IProjectileObject)this).UpdateTransform(projectile.Position, projectile.Rotation);
            _isPlaying = true;
            OnStartProjectile();
        }

        /// <summary>
        /// Projectileの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IProjectileObject.UpdateProjectile(float deltaTime) {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 飛翔終了処理
        /// </summary>
        void IProjectileObject.ExitProjectile() {
            if (!_isPlaying) {
                return;
            }

            IEnumerator Routine() {
                yield return ExitProjectileRoutine();
                Projectile = null;
                _isPlaying = false;
            }

            _coroutineRunner.StartCoroutine(Routine());
        }

        /// <summary>
        /// Transformの更新
        /// </summary>
        /// <param name="position">更新後の座標</param>
        /// <param name="rotation">更新後の回転</param>
        void IProjectileObject.UpdateTransform(Vector3 position, Quaternion rotation) {
            var trans = transform;
            trans.position = position;
            trans.rotation = rotation;
        }

        /// <summary>
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void IProjectileObject.OnHitCollision(RaycastHitResult result) {
            OnHitCollision(result);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected virtual void OnStartProjectile() {
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected virtual IEnumerator ExitProjectileRoutine() {
            yield break;
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        protected virtual void OnHitCollision(RaycastHitResult result) {
        }
    }
}