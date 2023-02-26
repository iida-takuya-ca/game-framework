using System;
using System.Reflection;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ModelSystems
{
    /// <summary>
    /// 自動割り当てId管理によるモデル
    /// </summary>
    public abstract class SingleModel<TModel> : IModel
        where TModel : SingleModel<TModel>
    {
        /// <summary>
        /// GenericTypeCache
        /// </summary>
        private static class TypeCache<T>
        {
            // コンストラクタ
            public static ConstructorInfo ConstructorInfo { get; }
            
            static TypeCache()
            {
                ConstructorInfo = typeof(T).GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, null, new [] { typeof(object) }, null);
            }
        }
        
        /// <summary>
        /// モデル格納用ストレージ
        /// </summary>
        private class Storage
        {
            // 管理対象のモデル
            private TModel _model;

            /// <summary>
            /// リセット処理
            /// </summary>
            public void Reset()
            {
                var model = _model;
                if (model == null)
                {
                    return;
                }

                _model = null;
                model.OnDeleted();
            }

            /// <summary>
            /// モデルの生成
            /// </summary>
            public TModel Create()
            {
                if (_model != null)
                {
                    Debug.LogError($"Already exists {typeof(TModel).Name}.");
                    return null;
                }

                var constructor = TypeCache<TModel>.ConstructorInfo;
                if (constructor == null)
                {
                    Debug.LogError($"Not found constructor. {typeof(TModel).Name}");
                    return null;
                }
                
                var model = (TModel)constructor.Invoke(new object[] { null });
                model._scope = new DisposableScope();
                model.OnCreatedInternal(model._scope);
                _model = model;
                return model;
            }

            /// <summary>
            /// モデルの取得
            /// </summary>
            public TModel Get()
            {
                return _model;
            }

            /// <summary>
            /// モデルの削除
            /// </summary>
            public void Delete()
            {
                var model = _model;
                if (model == null)
                {
                    return;
                }

                _model = null;
                model.OnDeleted();
                model._scope.Dispose();
                model._scope = null;
            }
        }

        // インスタンス管理用クラス
        private static Storage s_storage = new Storage();

        // 生成中のスコープ
        private DisposableScope _scope;

        // スコープ通知用
        public event Action OnExpired;

        /// <summary>
        /// 取得 or 生成処理
        /// </summary>
        public static TModel GetOrCreate()
        {
            var model = Get();
            if (model == null)
            {
                model = Create();
            }

            return model;
        }

        /// <summary>
        /// 取得処理
        /// </summary>
        public static TModel Get()
        {
            return s_storage.Get();
        }

        /// <summary>
        /// 生成処理
        /// </summary>
        public static TModel Create()
        {
            return s_storage.Create();
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        public static void Delete()
        {
            s_storage.Delete();
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        public static void Reset()
        {
            s_storage.Reset();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose()
        {
            Delete();
        }

        /// <summary>
        /// 生成時処理(Override用)
        /// </summary>
        protected virtual void OnCreatedInternal(IScope scope)
        {
        }

        /// <summary>
        /// 削除時処理(Override用)
        /// </summary>
        protected virtual void OnDeletedInternal()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="empty">デフォルトコンストラクタを無効にするための空引数</param>
        protected SingleModel(object empty)
        {
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        private void OnDeleted()
        {
            OnDeletedInternal();
            OnExpired?.Invoke();
            OnExpired = null;
        }
    }
}