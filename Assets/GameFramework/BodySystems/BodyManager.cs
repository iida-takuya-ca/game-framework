using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body管理クラス
    /// </summary>
    public class BodyManager : ILateUpdatableTask, IDisposable {
        /// <summary>
        /// Body情報
        /// </summary>
        private class BodyInfo {
            public IBody body;
            public bool disposed;
        }

        // 構築クラス
        private IBodyBuilder _builder;
        // 時間管理クラス
        private LayeredTime _layeredTime;
        // インスタンス管理
        private List<BodyInfo> _bodyInfos = new List<BodyInfo>();
        
        // アクティブ状態
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="builder">Body構築用クラス</param>
        /// <param name="layeredTime">時間管理クラス</param>
        public BodyManager(IBodyBuilder builder, LayeredTime layeredTime = null) {
            _builder = builder;
            _layeredTime = layeredTime;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            for (var i = 0; i < _bodyInfos.Count; i++) {
                var bodyInfo = _bodyInfos[i];
                if (bodyInfo.disposed) {
                    continue;
                }
                bodyInfo.body.Dispose();
            }
            _bodyInfos.Clear();
        }

        /// <summary>
        /// InstantiateされているGameObjectからBodyを作成する
        /// </summary>
        public Body CreateFromGameObject(GameObject gameObject) {
            var body = new Body(gameObject);
            
            // 構築処理
            BuildDefault(body);
            if (_builder != null) {
                _builder.Build(body);
            }
            
            // Dispatcherの生成
            var dispatcher = gameObject.AddComponent<BodyDispatcher>();
            dispatcher.Initialize(body);

            // 登録情報の初期化
            var bodyInfo = new BodyInfo {
                body = body,
                disposed = false
            };
            bodyInfo.body.OnExpired += () => {
                bodyInfo.disposed = true;
            };
            _bodyInfos.Add(bodyInfo);
            
            // Bodyの初期化
            bodyInfo.body.Initialize();

            return body;
        }

        /// <summary>
        /// PrefabからBodyを作成する
        /// </summary>
        public Body CreateFromPrefab(GameObject prefab) {
            var gameObject = Object.Instantiate(prefab);
            gameObject.name = prefab.name;
            return CreateFromGameObject(gameObject);
        }

        /// <summary>
        /// タスク更新処理
        /// </summary>
        void ITask.Update() {
            var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;
            
            for (var i = 0; i < _bodyInfos.Count; i++) {
                var bodyInfo = _bodyInfos[i];
                if (bodyInfo.disposed) {
                    continue;
                }
                bodyInfo.body.Update(deltaTime);
            }
        }

        /// <summary>
        /// タスク後更新処理
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;
            
            for (var i = 0; i < _bodyInfos.Count; i++) {
                var bodyInfo = _bodyInfos[i];
                if (bodyInfo.disposed) {
                    continue;
                }
                bodyInfo.body.LateUpdate(deltaTime);
            }
            
            // BodyInfosのリフレッシュ
            RefreshBodyInfos();
        }

        /// <summary>
        /// デフォルトのBody構築処理
        /// </summary>
        /// <param name="body">構築対象のBody</param>
        private void BuildDefault(IBody body) {
            body.AddController(new LocatorController());
        }

        /// <summary>
        /// BodyInfosの中身をリフレッシュ
        /// </summary>
        private void RefreshBodyInfos() {
            for (var i = _bodyInfos.Count - 1; i >= 0; i--) {
                var bodyInfo = _bodyInfos[i];
                if (!bodyInfo.disposed) {
                    continue;
                }
                _bodyInfos.RemoveAt(i);
            }
        }
    }
}