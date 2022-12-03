using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// ActorをEntityと紐づけるためのComponent
    /// </summary>
    public class ActorEntityComponent : EntityComponent {
        // 優先度で並べたActorリスト
        private readonly List<IActor> _sortedActors = new List<IActor>();

        /// <summary>
        /// 現在のActorを取得
        /// </summary>
        public TActor GetCurrentActor<TActor>()
            where TActor : Actor {
            if (_sortedActors.Count <= 0) {
                return default;
            }

            return _sortedActors[0] as TActor;
        }

        /// <summary>
        /// Actorを取得
        /// </summary>
        public TActor GetActor<TActor>()
            where TActor : Actor {
            var type = typeof(TActor);
            foreach (var actor in _sortedActors) {
                if (type.IsInstanceOfType(actor)) {
                    return (TActor)actor;
                }
            }

            return null;
        }

        /// <summary>
        /// Actorの追加
        /// </summary>
        public Entity AddActor(Actor actor) {
            _sortedActors.Add(actor);
            _sortedActors.Sort((a, b) => b.Priority - a.Priority);
            RefreshActiveActors();
            return Entity;
        }

        /// <summary>
        /// Actorの削除
        /// </summary>
        public Entity RemoveActor(Actor actor) {
            if (!_sortedActors.Remove(actor)) {
                return Entity;
            }

            actor.Deactivate();
            actor.Dispose();
            RefreshActiveActors();
            return Entity;
        }

        /// <summary>
        /// Actorの全削除
        /// </summary>
        public Entity RemoveActors() {
            foreach (var actor in _sortedActors) {
                actor.Dispose();
            }

            _sortedActors.Clear();
            return Entity;
        }

        /// <summary>
        /// アクティブ化された時の処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            RefreshActiveActors();
        }

        /// <summary>
        /// 非アクティブ化された時の処理
        /// </summary>
        protected override void DeactivateInternal() {
            RefreshActiveActors();
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            RemoveActors();
        }

        /// <summary>
        /// ActorのActive状態更新
        /// </summary>
        private void RefreshActiveActors() {
            var entityActive = Entity.IsActive;
            for (var i = 1; i < _sortedActors.Count; i++) {
                var actor = (Actor)_sortedActors[i];
                actor.Deactivate();
            }

            if (_sortedActors.Count > 0) {
                var actor = (Actor)_sortedActors[0];
                if (entityActive) {
                    actor.Activate();
                }
                else {
                    actor.Deactivate();
                }
            }
        }
    }
}