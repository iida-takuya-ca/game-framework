using System;
using System.Collections.Generic;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// エンティティ用のUtility
    /// </summary>
    public static class EntityExtensions {
        /// <summary>
        /// 基本的なEntityの初期化処理
        /// </summary>
        /// <param name="source">初期化対象のEntity</param>
        /// <param name="onCreateBody">Body生成</param>
        /// <param name="onSetupEntity">各種初期化処理</param>
        public static IObservable<Entity> SetupAsync(this Entity source, Func<IObservable<Body>> onCreateBody, Func<Entity, IObservable<Unit>> onSetupEntity) {
            return Observable.Defer(() => {
                // Entityの初期化
                var bodyEntityComponent = source.AddOrGetComponent<BodyEntityComponent>();
                source.AddOrGetComponent<LogicEntityComponent>();
                source.AddOrGetComponent<ModelEntityComponent>();

                var streams = new List<IObservable<Unit>>();
                
                // Bodyの生成
                if (onCreateBody != null) {
                    streams.Add(onCreateBody.Invoke()
                        .Do(body => bodyEntityComponent.SetBody(body))
                        .AsUnitObservable()
                    );
                }
                
                // Body生成後の初期化
                if (onSetupEntity != null) {
                    streams.Add(onSetupEntity.Invoke(source));
                }

                return streams.Concat()
                    .AsSingleUnitObservable()
                    .Select(_ => source);
            });
        }

        /// <summary>
        /// プレイヤーエンティティの初期化処理
        /// </summary>
        public static IObservable<Entity> SetupPlayerAsync(this Entity source, string assetKey) {
            return source.SetupAsync(() => {
                return new PlayerPrefabAssetRequest(assetKey)
                    .LoadAsync()
                    .Select(prefab => Services.Get<BodyManager>().CreateFromPrefab(prefab));
            }, entity => Observable.ReturnUnit());
        }

        /// <summary>
        /// Bodyの検索
        /// </summary>
        public static Body FindBody(this Entity source) {
            var component = source.GetComponent<BodyEntityComponent>();
            if (component == null) {
                return null;
            }
            return component.Body;
        }

        /// <summary>
        /// Logicの検索
        /// </summary>
        public static T FindLogic<T>(this Entity source)
            where T : EntityLogic {
            var component = source.GetComponent<LogicEntityComponent>();
            if (component == null) {
                return null;
            }
            return component.GetLogic<T>();
        }

        /// <summary>
        /// Modelの検索
        /// </summary>
        public static T FindModel<T>(this Entity source)
            where T : IModel {
            var component = source.GetComponent<ModelEntityComponent>();
            if (component == null) {
                return default;
            }
            return component.GetModel<T>();
        }
    }
}