using System;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// Entity拡張用コンポーネント基底
    /// </summary>
    public abstract class EntityComponent : IEntityComponent, IScope {
        // Attach中のScope
        private DisposableScope _attachScope = new DisposableScope();
        // Active中のScope
        private DisposableScope _activeScope = new DisposableScope();

        // Scope終了通知
        public event Action OnExpired;

        // AttachされているEntity
        public Entity Entity { get; private set; } = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [Preserve]
        public EntityComponent() {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            DisposeInternal();
            _activeScope.Dispose();
            _activeScope = null;
            _attachScope.Dispose();
            _attachScope = null;
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
            AttachedInternal(_attachScope);
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        void IEntityComponent.Activate() {
            ActivateInternal(_activeScope);
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void IEntityComponent.Deactivate() {
            DeactivateInternal();
            _activeScope.Clear();
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
            _attachScope.Clear();
            Entity = null;
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// Entityに登録された時の処理
        /// </summary>
        protected virtual void AttachedInternal(IScope scope) {
        }

        /// <summary>
        /// アクティブ化時の処理
        /// </summary>
        protected virtual void ActivateInternal(IScope scope) {
        }

        /// <summary>
        /// 非アクティブ化時の処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }

        /// <summary>
        /// Entityから登録解除された時の処理
        /// </summary>
        protected virtual void DetachedInternal() {
        }
    }
}