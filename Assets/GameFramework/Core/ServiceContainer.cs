using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のコンテナ
    /// </summary>
    public class ServiceContainer : IServiceContainer {
        // 管理用サービス
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private Dictionary<Type, List<object>> _serviceLists = new Dictionary<Type, List<object>>();
        
        // 登録解除用の廃棄可能インスタンスリスト(登録順)
        private List<IDisposable> _disposableServices = new List<IDisposable>();

        // 自動Disposeフラグ
        private bool _autoDispose;

        // 親のContainer
        private IServiceContainer _parent;
        // 子のContainer
        private List<IServiceContainer> _children = new List<IServiceContainer>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親ServiceContainer</param>
        /// <param name="autoDispose">登録したServiceを自動Disposeするか</param>
        public ServiceContainer(IServiceContainer parent = null, bool autoDispose = true) {
            if (parent == null && GetType() != typeof(Services)) {
                parent = Services.Instance;
            } 
            
            _autoDispose = autoDispose;
            _parent = parent;
            
            if (_parent is ServiceContainer container) {
                container._children.Add(this);
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            // 子を解放
            for (var i = _children.Count - 1; i >= 0; i--) {
                _children[i].Dispose();
            }
            _children.Clear();
            
            if (_autoDispose) {
                // 逆順に解放
                for (var i = _disposableServices.Count - 1; i >= 0; i--) {
                    var disposable = _disposableServices[i];
                    if (disposable == null) {
                        continue;
                    }
                    disposable.Dispose();
                }
            }
            
            // サービス参照をクリア
            _disposableServices.Clear();
            _services.Clear();
            _serviceLists.Clear();
        }

        /// <summary>
        /// サービスの設定
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        public void Set(object service) {
            var type = service.GetType();
            if (_services.ContainsKey(type)) {
                Debug.LogError($"Already set service. Type:{type}");
                return;
            }

            _services[type] = service;
            if (service is IDisposable disposable) {
                _disposableServices.Add(disposable);
            }
        }

        /// <summary>
        /// サービスの設定(複数登録するバージョン）
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        /// <param name="index">インデックス</param>
        public void Set(object service, int index) {
            var type = service.GetType();
            if (!_serviceLists.TryGetValue(type, out var list)) {
                list = new List<object>();
                _serviceLists[type] = list;
            }

            while (index >= list.Count) {
                list.Add(null);
            }

            if (list[index] != null) {
                Debug.LogError($"Already set service. Type:{type} Index:{index}");
                return;
            }

            list[index] = service;
            if (service is IDisposable disposable) {
                _disposableServices.Add(disposable);
            }
        }

        /// <summary>
        /// サービスの取得
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// </summary>
        object IServiceContainer.Get(Type type) {
            for (var i = _children.Count - 1; i >= 0; i--) {
                var result = _children[i].Get(type);
                if (result != default) {
                    return result;
                }
            }
            
            if (_services.TryGetValue(type, out var service)) {
                return service;
            }

            foreach (var pair in _services) {
                if (type.IsAssignableFrom(pair.Key)) {
                    return pair.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// サービスの取得
        /// </summary>
        T IServiceContainer.Get<T>() {
            return (T)((IServiceContainer)this).Get(typeof(T));
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <param name="index">インデックス</param>
        object IServiceContainer.Get(Type type, int index) {
            for (var i = _children.Count - 1; i >= 0; i--) {
                var result = _children[i].Get(type, index);
                if (result != default) {
                    return result;
                }
            }

            _serviceLists.TryGetValue(type, out var list);

            if (list == null) {
                foreach (var pair in _serviceLists) {
                    if (type.IsAssignableFrom(pair.Key)) {
                        list = pair.Value;
                    }
                }
            }

            if (list != null) {
                if (index < list.Count) {
                    return list[index];
                }
                Debug.LogError($"Invalid service index. Type:{type} Index:{index}");
            }

            return null;
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="index">インデックス</param>
        T IServiceContainer.Get<T>(int index) {
            return (T)((IServiceContainer)this).Get(typeof(T), index);
        }
    }
}
