using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;

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
        public static async UniTask<Entity> SetupAsync(this Entity source, Func<UniTask<Body>> onCreateBody,
            Action<Entity> onSetupEntity) {
            // Bodyの生成
            if (onCreateBody != null) {
                var body = await onCreateBody.Invoke();
                source.SetBody(body);
            }

            // Body生成後の初期化
            if (onSetupEntity != null) {
                onSetupEntity.Invoke(source);
            }

            return source;
        }

        /// <summary>
        /// プレイヤーエンティティの初期化処理
        /// </summary>
        public static async UniTask<Entity> SetupPlayerAsync(this Entity source, Battle.BattlePlayerModel model, IScope unloadScope, CancellationToken ct) {
            return await source.SetupAsync(async () => {
                var prefab = await new PlayerPrefabAssetRequest(model.AssetKey)
                    .LoadAsync(unloadScope, ct);
                return Services.Get<BodyManager>().CreateFromPrefab(prefab);
            }, entity => {
                var actor = new PlayerActor(entity.GetBody(), model.ActorModel.Setup);
                actor.RegisterTask(TaskOrder.Actor);
                var logic = new Battle.BattlePlayerPresenter(actor, model);
                logic.RegisterTask(TaskOrder.Logic);
                entity.AddActor(actor)
                    .AddLogic(logic);
            });
        }
    }
}