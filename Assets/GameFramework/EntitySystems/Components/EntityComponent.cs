using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// Entity拡張用コンポーネント基底
    /// </summary>
    public abstract class EntityComponent : IEntityComponent, IScope {
        // Attach中のScope
        private DisposableScope _attachScope;

        // Scope終了通知
        public event Action OnExpired;
        
        // AttachされているEntity
        public Entity Entity { get; private set; } = null;
        
        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            DisposeInternal();
            OnExpired?.Invoke();
            OnExpired = null;
        }

        /// <summary>
        /// Entityに登録された時の処理
        /// </summary>
        /// <param name="entity">対象のEntity</param>
        void IEntityComponent.Attached(Entity entity) {
            if (Entity != null) {
                Debug.LogError($"Already attached component. {GetType().Name}");
                return;
            }
            Entity = entity;
            _attachScope = new DisposableScope();
            AttachedInternal(_attachScope);
        }

        /// <summary>
        /// Entityから登録解除された時の処理
        /// </summary>
        /// <param name="entity">対象のEntity</param>
        void IEntityComponent.Detached(Entity entity) {
            if (entity != Entity) {
                Debug.LogError($"Invalid detached entity. {GetType().Name}");
                return;
            }
            DetachedInternal();
            _attachScope.Dispose();
            _attachScope = null;
            Entity = null;
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {}

        /// <summary>
        /// Entityに登録された時の処理
        /// </summary>
        protected virtual void AttachedInternal(IScope scope) {}

        /// <summary>
        /// Entityから登録解除された時の処理
        /// </summary>
        protected virtual void DetachedInternal() {}
    }
}