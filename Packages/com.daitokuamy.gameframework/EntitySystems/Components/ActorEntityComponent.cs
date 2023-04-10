using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// ActorをEntityと紐づけるためのComponent
    /// </summary>
    public class ActorEntityComponent : EntityComponent {
        /// <summary>
        /// Actor管理情報
        /// </summary>
        private class ActorInfo {
            public int priority;
            public IActor actor;
        }

        // 優先度で並べたActor情報リスト
        private readonly List<ActorInfo> _sortedActorInfos = new List<ActorInfo>();

        /// <summary>
        /// 現在のActorを取得
        /// </summary>
        public TActor GetCurrentActor<TActor>()
            where TActor : Actor {
            if (_sortedActorInfos.Count <= 0) {
                return default;
            }

            return _sortedActorInfos[0].actor as TActor;
        }

        /// <summary>
        /// Actorを取得
        /// </summary>
        public TActor GetActor<TActor>()
            where TActor : Actor {
            var type = typeof(TActor);
            foreach (var actorInfo in _sortedActorInfos) {
                var actor = actorInfo.actor;
                if (type.IsInstanceOfType(actor)) {
                    return (TActor)actor;
                }
            }

            return null;
        }

        /// <summary>
        /// Actorの追加
        /// </summary>
        public Entity AddActor(Actor actor, int priority = 0) {
            var info = new ActorInfo { actor = actor, priority = priority };
            _sortedActorInfos.Add(info);
            _sortedActorInfos.Sort((a, b) => b.priority - a.priority);
            RefreshActiveActors();
            return Entity;
        }

        /// <summary>
        /// Actorの削除
        /// </summary>
        public Entity RemoveActor(Actor actor, bool dispose = true) {
            var count = _sortedActorInfos.RemoveAll(x => x.actor == actor);
            if (count <= 0) {
                return Entity;
            }

            actor.Deactivate();
            if (dispose) {
                actor.Dispose();
            }

            RefreshActiveActors();
            return Entity;
        }

        /// <summary>
        /// Actorの全削除
        /// </summary>
        public Entity RemoveActors(bool dispose = true) {
            foreach (var actorInfo in _sortedActorInfos) {
                ((Actor)actorInfo.actor).Deactivate();
                if (dispose) {
                    actorInfo.actor.Dispose();
                }
            }

            _sortedActorInfos.Clear();
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
            for (var i = 1; i < _sortedActorInfos.Count; i++) {
                var actor = (Actor)_sortedActorInfos[i].actor;
                actor.Deactivate();
            }

            if (_sortedActorInfos.Count > 0) {
                var actor = (Actor)_sortedActorInfos[0].actor;
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