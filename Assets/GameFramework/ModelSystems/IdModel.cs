using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GameFramework.ModelSystems
{
    /// <summary>
    /// Id管理によるモデル
    /// </summary>
    public abstract class IdModel<TKey, TModel> : IModel
        where TModel : IdModel<TKey, TModel>, new()
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
                ConstructorInfo = typeof(T).GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, null, new [] { typeof(TKey) }, null);
            }
        }
        
        /// <summary>
        /// モデル格納用ストレージ
        /// </summary>
        private class Storage
        {
            // 管理対象のモデル
            private Dictionary<TKey, TModel> _models = new Dictionary<TKey, TModel>();

            /// <summary>
            /// リセット処理
            /// </summary>
            public void Reset()
            {
                var keys = _models.Keys.ToArray();
                for (var i = 0; i < keys.Length; i++)
                {
                    var model = _models[keys[i]];
                    if (model == null)
                    {
                        return;
                    }

                    _models.Remove(keys[i]);
                    model.OnDeleted();
                }
            }

            /// <summary>
            /// モデルの生成
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public TModel Create(TKey id)
            {
                if (_models.ContainsKey(id))
                {
                    Debug.LogError($"Already exists {typeof(TModel).Name}. key:{id}");
                    return null;
                }

                var constructor = TypeCache<TModel>.ConstructorInfo;
                if (constructor == null)
                {
                    Debug.LogError($"Not found constructor. {typeof(TModel).Name}");
                    return null;
                }
                
                var model = (TModel)constructor.Invoke(new object[] { id });
                model.OnCreatedInternal();
                _models[id] = model;
                return model;
            }

            /// <summary>
            /// モデルの取得
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public TModel Get(TKey id)
            {
                if (!_models.TryGetValue(id, out var model))
                {
                    return null;
                }

                return model;
            }

            /// <summary>
            /// モデルの削除
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public void Delete(TKey id)
            {
                if (!_models.TryGetValue(id, out var model))
                {
                    return;
                }

                _models.Remove(id);
                model.OnDeleted();
            }
        }

        // インスタンス管理用クラス
        private static Storage s_storage = new Storage();

        // 識別ID
        public TKey Id { get; private set; }

        // スコープ通知用
        public event Action OnExpired;

        /// <summary>
        /// 取得 or 生成処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel GetOrCreate(TKey id)
        {
            var model = Get(id);
            if (model == null)
            {
                model = Create(id);
            }

            return model;
        }

        /// <summary>
        /// 取得処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel Get(TKey id)
        {
            return s_storage.Get(id);
        }

        /// <summary>
        /// 生成処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel Create(TKey id)
        {
            return s_storage.Create(id);
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static void Delete(TKey id)
        {
            s_storage.Delete(id);
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
            Delete(Id);
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
        protected IdModel(TKey id)
        {
            Id = id;
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        private void OnDeleted()
        {
            OnDeletedInternal();
            Id = default;
            OnExpired?.Invoke();
            OnExpired = null;
        }
    }
}