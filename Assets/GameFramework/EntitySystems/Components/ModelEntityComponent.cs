using System;
using System.Collections.Generic;
using GameFramework.ModelSystems;

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
        public void SetModel(IModel model) {
            var type = model.GetType();
            _models[type] = model;
        }
        
        /// <summary>
        /// モデルのクリア
        /// </summary>
        public void RemoveModel<TModel>()
        where TModel : IModel {
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