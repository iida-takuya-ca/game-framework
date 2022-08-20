using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のロケーター
    /// </summary>
    public class ServiceLocator : IServiceLocator {
        // 管理用サービス
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private Dictionary<Type, List<object>> _serviceLists = new Dictionary<Type, List<object>>();
        
        // 登録解除用の廃棄可能インスタンスリスト(登録順)
        private List<IDisposable> _disposableServices = new List<IDisposable>();

        // 自動Disposeフラグ
        private bool _autoDispose;

        // 親のLocator
        private IServiceLocator _parent;
        // 子のLocator
        private List<IServiceLocator> _children = new List<IServiceLocator>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親ServiceLocator</param>
        /// <param name="autoDispose">登録したServiceを自動Disposeするか</param>
        public ServiceLocator(IServiceLocator parent, bool autoDispose = true) {
            _autoDispose = autoDispose;
            _parent = parent;
            
            if (_parent is ServiceLocator locator) {
                locator._children.Add(this);
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
        public object Get(Type type) {
            if (!_services.TryGetValue(type, out var service)) {
                // 親を再帰的に検索
                if (_parent != null) {
                    return _parent.Get(type);
                }
                return default;
            }

            return service;
        }

        /// <summary>
        /// サービスの取得
        /// </summary>
        public T Get<T>() {
            return (T)Get(typeof(T));
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <param name="index">インデックス</param>
        public object Get(Type type, int index) {
            if (!_serviceLists.TryGetValue(type, out var list)) {
                // 親を再帰的に検索
                if (_parent != null) {
                    return _parent.Get(type, index);
                }
                return default;
            }

            if (index >= list.Count) {
                Debug.LogError($"Invalid service index. Type:{type} Index:{index}");
                return default;
            }

            return list[index];
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="index">インデックス</param>
        public T Get<T>(int index) {
            return (T)Get(typeof(T), index);
        }
    }
}
