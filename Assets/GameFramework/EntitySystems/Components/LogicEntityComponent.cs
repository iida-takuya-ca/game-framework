using System;
using System.Collections.Generic;
using GameFramework.LogicSystems;
using UnityEngine;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// LogicをEntityと紐づけるためのComponent
    /// </summary>
    public class LogicEntityComponent : EntityComponent {
        // ロジックのキャッシュ
        private Dictionary<Type, EntityLogic> _logics = new Dictionary<Type, EntityLogic>();

        /// <summary>
        /// ロジックの取得
        /// </summary>
        public TLogic GetLogic<TLogic>()
            where TLogic : EntityLogic {
            var type = typeof(TLogic);
            if (_logics.TryGetValue(type, out var logic)) {
                return (TLogic)logic;
            }

            foreach (var pair in _logics) {
                if (type.IsAssignableFrom(pair.Key)) {
                    return (TLogic)pair.Value;
                }
            }

            return default;
        }

        /// <summary>
        /// ロジックの追加(Remove時に自動削除)
        /// </summary>
        public Entity AddLogic<TLogic>(TLogic logic)
            where TLogic : EntityLogic {
            var type = typeof(TLogic);
            if (_logics.ContainsKey(type)) {
                Debug.LogError($"Already exists logic. type:{type.Name}");
                return Entity;
            }

            if (logic.Entity != null) {
                Debug.LogError($"Entity is not null. type:{type.Name}");
                return Entity;
            }

            _logics[type] = logic;
            logic.Attach(Entity);
            if (Entity.IsActive) {
                logic.Activate();
            }

            return Entity;
        }

        /// <summary>
        /// ロジックの削除
        /// </summary>
        public Entity RemoveLogic<TLogic>()
            where TLogic : Logic {
            var type = typeof(TLogic);
            if (!_logics.TryGetValue(type, out var logic)) {
                return Entity;
            }

            _logics.Remove(typeof(TLogic));
            logic.Deactivate();
            logic.Detach(Entity);
            logic.Dispose();
            return Entity;
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            foreach (var logic in _logics.Values) {
                logic.Dispose();
            }

            _logics.Clear();
        }
    }
}