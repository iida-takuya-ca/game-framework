using System;
using System.Collections.Generic;
using GameFramework.ModelSystems;
using UnityEngine;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// ModelをEntityと紐づけるためのComponent
    /// </summary>
    public class ModelEntityComponent : EntityComponent {
        // モデルのキャッシュ
        private Dictionary<Type, IModel> _models = new Dictionary<Type, IModel>();

        /// <summary>
        /// モデルの取得
        /// </summary>
        public TModel GetModel<TModel>()
            where TModel : IModel {
            if (_models.TryGetValue(typeof(TModel), out var model)) {
                return (TModel)model;
            }

            return default;
        }

        /// <summary>
        /// モデルの設定
        /// </summary>
        public TModel SetModel<TModel>(TModel model)
            where TModel : class, IModel {
            var type = typeof(TModel);
            if (_models.ContainsKey(type)) {
                Debug.LogError($"Already exists model. type:{type.Name}");
                return null;
            }
            
            _models[type] = model;
            return model;
        }
        
        /// <summary>
        /// モデルの設定クリア（削除はされない）
        /// </summary>
        public void ClearModel<TModel>()
        where TModel : IModel {
            var type = typeof(TModel);
            if (!_models.TryGetValue(type, out var model)) {
                return;
            }
            
            _models.Remove(typeof(TModel));
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            _models.Clear();
        }
    }
}