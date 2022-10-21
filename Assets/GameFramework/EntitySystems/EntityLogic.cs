using GameFramework.Core;
using GameFramework.LogicSystems;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// エンティティ用ロジック処理
    /// </summary>
    public abstract class EntityLogic : Logic
    {
        // アタッチ中のScope
        private DisposableScope _attachScope;
        
        // 持ち主のEntity
        public Entity Entity { get; private set; }

        /// <summary>
        /// Entityに追加された時の処理
        /// </summary>
        /// <param name="entity">持ち主のEntity</param>
        public void Attach(Entity entity)
        {
            if (Entity != null || entity == null)
            {
                return;
            }
            
            Entity = entity;
            _attachScope = new DisposableScope();
            AttachInternal();
        }

        /// <summary>
        /// Entityから削除された時の処理
        /// </summary>
        /// <param name="entity">持ち主のEntity</param>
        public void Detach(Entity entity)
        {
            if (Entity == null || entity == null)
            {
                return;
            }
            
            if (entity == Entity)
            {
                DetachInternal();
                _attachScope.Dispose();
                _attachScope = null;
                Entity = null;
            }
        }

        /// <summary>
        /// Entityに追加された時の処理
        /// </summary>
        protected virtual void AttachInternal()
        {
        }

        /// <summary>
        /// Entityから削除された時の処理
        /// </summary>
        /// <returns></returns>
        protected virtual void DetachInternal()
        {
        }
    }
}