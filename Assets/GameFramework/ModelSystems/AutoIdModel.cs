using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameFramework.ModelSystems {
    /// <summary>
    /// 自動割り当てId管理によるモデル
    /// </summary>
    public abstract class AutoIdModel<TModel> : IModel
        where TModel : AutoIdModel<TModel>
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
                ConstructorInfo = typeof(T).GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, null, new [] { typeof(int) }, null);
            }
        }
        
        /// <summary>
        /// モデル格納用ストレージ
        /// </summary>
        private class Storage
        {
            private int _nextId = 1;

            // 管理対象のモデル
            private List<TModel> _models = new List<TModel>();

            /// <summary>
            /// リセット処理
            /// </summary>
            public void Reset()
            {
                for (var i = 0; i < _models.Count; i++)
                {
                    var model = _models[i];
                    if (model == null)
                    {
                        continue;
                    }

                    _models[i] = null;
                    model.OnDeleted();
                }

                _models.Clear();
                _nextId = 1;
            }

            /// <summary>
            /// モデルの生成
            /// </summary>
            public TModel Create()
            {
                var constructor = TypeCache<TModel>.ConstructorInfo;
                if (constructor == null)
                {
                    Debug.LogError($"Not found constructor. {typeof(TModel).Name}");
                    return null;
                }
                
                var id = _nextId++;
                var model = (TModel)constructor.Invoke(new object[] { id });
                model.OnCreatedInternal();
                _models.Add(model);
                return model;
            }

            /// <summary>
            /// モデルの取得
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public TModel Get(int id)
            {
                if (id > _models.Count)
                {
                    return null;
                }

                return _models[IdToIndex(id)];
            }

            /// <summary>
            /// モデルの削除
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public void Delete(int id)
            {
                if (id > _models.Count)
                {
                    return;
                }

                var index = IdToIndex(id);
                var model = _models[index];
                if (model == null)
                {
                    return;
                }

                _models[index] = null;
                model.OnDeleted();
            }

            /// <summary>
            /// IdをIndexに変換
            /// </summary>
            private int IdToIndex(int id)
            {
                return id - 1;
            }
        }

        // インスタンス管理用クラス
        private static Storage s_storage = new Storage();

        // 識別ID
        public int Id { get; private set; }

        // スコープ通知用
        public event Action OnExpired;

        /// <summary>
        /// 取得処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel Get(int id)
        {
            return s_storage.Get(id);
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
        /// <param name="id">識別キー</param>
        public static void Delete(int id)
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
        protected AutoIdModel(int id)
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