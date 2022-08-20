using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body(見た目制御クラス)
    /// </summary>
    public class Body : IBody {
        // BodyControllerリスト
        private Dictionary<Type, IBodyController> _bodyControllers = new Dictionary<Type, IBodyController>();
        // 並び順に並べられたControllerリスト
        private List<IBodyController> _orderedBodyControllers = new List<IBodyController>();

        // 解放スコープ
        public event Action OnExpired;
        
        // 制御対象のGameObject
        public GameObject GameObject { get; private set; }
        // 制御対象のTransform
        public Transform Transform { get; private set; }
        // 時間管理クラス
        public LayeredTime LayeredTime { get; } = new LayeredTime();
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Body(GameObject gameObject) {
            GameObject = gameObject;
            Transform = gameObject != null ? gameObject.transform : null;
        }
        
        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            OnExpired?.Invoke();
            OnExpired = null;
            
            LayeredTime.Dispose();
            
            // Controllerの削除
            for (var i = _orderedBodyControllers.Count - 1; i >= 0; i--) {
                var controller = _orderedBodyControllers[i];
                controller.Dispose();
            }
            _orderedBodyControllers.Clear();
            _bodyControllers.Clear();
            
            // GameObjectの削除
            if (GameObject != null) {
                BodyUtility.Destroy(GameObject);
            }
            GameObject = null;
            Transform = null;
        }

        /// <summary>
        /// BodyControllerの取得
        /// </summary>
        public T GetController<T>()
            where T : IBodyController {
            if (_bodyControllers.TryGetValue(typeof(T), out var controller)) {
                return (T)controller;
            }
            return default;
        }

        /// <summary>
        /// Component取得
        /// <param name="type">取得するタイプ</param>
        /// </summary>
        public Component GetComponent(Type type) {
            return GameObject.GetComponent(type);
        }

        /// <summary>
        /// Component取得
        /// </summary>
        public T GetComponent<T>()
            where T : Component {
            return GameObject.GetComponent<T>();
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        /// <param name="type">取得するタイプ</param>
        public Component[] GetComponents(Type type) {
            return GameObject.GetComponents(type);
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        public T[] GetComponents<T>()
            where T : Component {
            return GameObject.GetComponents<T>();
        }

        /// <summary>
        /// Component取得
        /// <param name="type">取得するタイプ</param>
        /// </summary>
        public Component GetComponentInChildren(Type type, bool includeInactive = false) {
            return GameObject.GetComponentInChildren(type, includeInactive);
        }

        /// <summary>
        /// Component取得
        /// </summary>
        public T GetComponentInChildren<T>(bool includeInactive = false)
            where T : Component {
            return GameObject.GetComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        /// <param name="type">取得するタイプ</param>
        public Component[] GetComponentsInChildren(Type type, bool includeInactive = false) {
            return GameObject.GetComponentsInChildren(type, includeInactive);
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        public T[] GetComponentsInChildren<T>(bool includeInactive = false)
            where T : Component {
            return GameObject.GetComponentsInChildren<T>(includeInactive);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IBody.Initialize() {
            _orderedBodyControllers.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
            
            // Controller初期化
            for (var i = 0; i < _orderedBodyControllers.Count; i++) {
                var controller = _orderedBodyControllers[i];
                controller.Initialize(this);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBody.Update(float deltaTime) {
            deltaTime *= LayeredTime.TimeScale;
            
            // Controller更新
            for (var i = 0; i < _orderedBodyControllers.Count; i++) {
                var controller = _orderedBodyControllers[i];
                controller.Update(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBody.LateUpdate(float deltaTime) {
            deltaTime *= LayeredTime.TimeScale;
            
            // Controller更新
            for (var i = 0; i < _orderedBodyControllers.Count; i++) {
                var controller = _orderedBodyControllers[i];
                controller.LateUpdate(deltaTime);
            }
        }

        /// <summary>
        /// BodyControllerの追加
        /// </summary>
        /// <param name="controller">対象のController</param>
        void IBody.AddController(IBodyController controller) {
            if (controller == null) {
                return;
            }
            
            var type = controller.GetType();
            if (_bodyControllers.ContainsKey(type)) {
                Debug.LogError($"Already added body controller type. [{GameObject.name}] > [{type}]");
                return;
            }

            _bodyControllers[type] = controller;
            _orderedBodyControllers.Add(controller);
        }
    }
}