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
        private Dictionary<Type, Logic> _logics = new Dictionary<Type, Logic>();

        /// <summary>
        /// ロジックの取得
        /// </summary>
        public TLogic GetLogic<TLogic>()
            where TLogic : Logic {
            if (_logics.TryGetValue(typeof(TLogic), out var model)) {
                return (TLogic)model;
            }

            return default;
        }

        /// <summary>
        /// ロジックの追加(Remove時に自動削除)
        /// </summary>
        public TLogic AddLogic<TLogic>(TLogic logic) 
            where TLogic : Logic {
            var type = typeof(TLogic);
            if (_logics.ContainsKey(type)) {
                Debug.LogError($"Already exists logic. type:{type.Name}");
                return null;
            }
            
            _logics[type] = logic;
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
            logic.Dispose();
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            _logics.Clear();
        }
    }
}