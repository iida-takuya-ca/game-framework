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
        public TLogic AddLogic<TLogic>(TLogic logic) 
            where TLogic : EntityLogic {
            var type = typeof(TLogic);
            if (_logics.ContainsKey(type)) {
                Debug.LogError($"Already exists logic. type:{type.Name}");
                return null;
            }

            if (logic.Entity != null)
            {
                Debug.LogError($"Entity is not null. type:{type.Name}");
                return null;
            }
            
            _logics[type] = logic;
            logic.Attach(Entity);
            return logic;
        }
        
        /// <summary>
        /// ロジックの削除
        /// </summary>
        public void RemoveLogic<TLogic>()
            where TLogic : Logic {
            var type = typeof(TLogic);
            if (!_logics.TryGetValue(type, out var logic)) {
                return;
            }
            
            _logics.Remove(typeof(TLogic));
            logic.Detach(Entity);
            logic.Dispose();
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            foreach (var logic in _logics.Values)
            {
                logic.Detach(Entity);
            }
            _logics.Clear();
        }
    }
}