using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// インスタンス管理用クラスのコア
    /// </summary>
    public class Entity : IDisposable {
        // Entity拡張用Component
        private Dictionary<Type, IEntityComponent> _components = new Dictionary<Type, IEntityComponent>();

        /// <summary>
        /// Componentの取得
        /// </summary>
        /// <param name="type">取得する型</param>
        public EntityComponent GetComponent(Type type) {
            // 型一致での検索
            if (!_components.TryGetValue(type, out var component)) {
                return null;
            }
            return (EntityComponent)component;
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        public T GetComponent<T>()
            where T : EntityComponent {
            return (T)GetComponent(typeof(T));
        }
        
        /// <summary>
        /// Componentの追加(重複はエラー)
        /// </summary>
        /// <param name="type">追加する型</param>
        public EntityComponent AddComponent(Type type) {
            if (!type.IsSubclassOf(typeof(EntityComponent))) {
                Debug.LogError($"Component is not EntityComponent. [{type.Name}]");
                return null;
            }
            if (_components.ContainsKey(type)) {
                Debug.LogError($"Already exists entity component. [{type.Name}]");
                return null;
            }
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null) {
                Debug.LogError($"Not found default constructor. [{type.Name}]");
                return null;
            }
            var component = (IEntityComponent)constructor.Invoke(Array.Empty<object>());
            _components[type] = component;
            component.Attached(this);
            return (EntityComponent)component;
        }

        /// <summary>
        /// Componentの追加(重複はエラー)
        /// </summary>
        public T AddComponent<T>()
            where T : EntityComponent {
            return (T)AddComponent(typeof(T));
        }

        /// <summary>
        /// Componentの追加、既に追加されていたら取得
        /// </summary>
        /// <param name="type">追加する型</param>
        public EntityComponent AddOrGetComponent(Type type) {
            var component = GetComponent(type);
            if (component == null) {
                component = AddComponent(type);
            }
            return component;
        }

        /// <summary>
        /// Componentの追加、既に追加されていたら取得
        /// </summary>
        public T AddOrGetComponent<T>()
            where T : EntityComponent {
            return (T)AddOrGetComponent(typeof(T));
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            foreach (var component in _components.Values) {
                component.Detached(this);
                component.Dispose();
            }
            _components.Clear();
        }
    }
}