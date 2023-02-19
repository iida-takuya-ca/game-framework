using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体の実態制御用MonoBehaviour
    /// </summary>
    public class ProjectileObject : MonoBehaviour {
        // 使用中のProjectile
        protected IProjectile Projectile { get; private set; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(IProjectile projectile) {
            Projectile = projectile;
            OnSetup();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Cleanup() {
            OnCleanup();
            Projectile = null;
        }

        /// <summary>
        /// Transformの更新
        /// </summary>
        /// <param name="position">更新後の座標</param>
        /// <param name="rotation">更新後の回転</param>
        public virtual void UpdateTransform(Vector3 position, Quaternion rotation) {
            var trans = transform;
            trans.position = position;
            trans.rotation = rotation;
        }

        /// <summary>
        /// 初期化通知
        /// </summary>
        protected virtual void OnSetup() {}

        /// <summary>
        /// 終了通知
        /// </summary>
        protected virtual void OnCleanup() {}
    }
}