using GameFramework.BodySystems;
using GameFramework.ModelSystems;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// エンティティの拡張メソッド
    /// </summary>
    public static class EntityExtensions {
        /// <summary>
        /// Bodyの検索
        /// </summary>
        public static Body GetBody(this Entity source) {
            var component = source.GetComponent<BodyEntityComponent>();
            if (component == null) {
                return null;
            }

            return component.Body;
        }

        /// <summary>
        /// Bodyの設定
        /// </summary>
        public static Entity SetBody(this Entity source, Body body, bool prevDispose = true) {
            var component = source.GetComponent<BodyEntityComponent>();
            if (component != null) {
                return component.SetBody(body, prevDispose);
            }

            return source;
        }

        /// <summary>
        /// Bodyの設定
        /// </summary>
        public static Entity RemoveBody(this Entity source, bool dispose = true) {
            var component = source.GetComponent<BodyEntityComponent>();
            if (component != null) {
                return component.RemoveBody(dispose);
            }

            return source;
        }

        /// <summary>
        /// 現在のActorを取得
        /// </summary>
        public static TActor GetCurrentActor<TActor>(this Entity source)
            where TActor : Actor {
            var component = source.GetComponent<ActorEntityComponent>();
            if (component == null) {
                return default;
            }

            return component.GetCurrentActor<TActor>();
        }

        /// <summary>
        /// Actorを取得
        /// </summary>
        public static TActor GetActor<TActor>(this Entity source)
            where TActor : Actor {
            var component = source.GetComponent<ActorEntityComponent>();
            if (component == null) {
                return default;
            }

            return component.GetActor<TActor>();
        }

        /// <summary>
        /// Actorを追加
        /// </summary>
        public static Entity AddActor(this Entity source, Actor actor) {
            var component = source.GetComponent<ActorEntityComponent>();
            if (component != null) {
                return component.AddActor(actor);
            }

            return source;
        }

        /// <summary>
        /// Actorの削除
        /// </summary>
        public static Entity RemoveActor(this Entity source, Actor actor) {
            var component = source.GetComponent<ActorEntityComponent>();
            if (component != null) {
                return component.RemoveActor(actor);
            }

            return source;
        }

        /// <summary>
        /// Actorの全削除
        /// </summary>
        public static Entity RemoveActors(this Entity source) {
            var component = source.GetComponent<ActorEntityComponent>();
            if (component != null) {
                return component.RemoveActors();
            }

            return source;
        }

        /// <summary>
        /// Logicの取得
        /// </summary>
        public static TLogic GetLogic<TLogic>(this Entity source)
            where TLogic : EntityLogic {
            var component = source.GetComponent<LogicEntityComponent>();
            if (component == null) {
                return null;
            }

            return component.GetLogic<TLogic>();
        }

        /// <summary>
        /// Logicを追加
        /// </summary>
        public static Entity AddLogic(this Entity source, EntityLogic logic) {
            var component = source.GetComponent<LogicEntityComponent>();
            if (component != null) {
                return component.AddLogic(logic);
            }

            return source;
        }

        /// <summary>
        /// Logicの削除
        /// </summary>
        public static Entity RemoveLogic<TLogic>(this Entity source)
            where TLogic : EntityLogic {
            var component = source.GetComponent<LogicEntityComponent>();
            if (component != null) {
                return component.RemoveLogic<TLogic>();
            }

            return source;
        }

        /// <summary>
        /// Modelの取得
        /// </summary>
        public static TModel GetModel<TModel>(this Entity source)
            where TModel : IModel {
            var component = source.GetComponent<ModelEntityComponent>();
            if (component == null) {
                return default;
            }

            return component.GetModel<TModel>();
        }

        /// <summary>
        /// Modelを追加
        /// </summary>
        public static Entity SetModel<TModel>(this Entity source, TModel model)
            where TModel : class, IModel {
            var component = source.GetComponent<ModelEntityComponent>();
            if (component != null) {
                return component.SetModel(model);
            }

            return source;
        }

        /// <summary>
        /// Modelの設定をクリア（削除はされない）
        /// </summary>
        public static Entity ClearModel<TModel>(this Entity source)
            where TModel : class, IModel {
            var component = source.GetComponent<ModelEntityComponent>();
            if (component != null) {
                return component.ClearModel<TModel>();
            }

            return source;
        }
    }
}