using System;
using System.Collections;
using System.Linq;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using GameFramework.ModelSystems;
using GameFramework.TaskSystems;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// バトル用モデル
    /// </summary>
    public class BattleModel : SingleModel<BattleModel>, ITask {
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();

        public event Action<BattleModel> OnUpdated;

        bool ITask.IsActive => true;
        public BattlePlayerModel PlayerModel { get; private set; }

        public IObservable<BattleModel> OnUpdatedAsObservable() {
            return Observable.FromEvent<BattleModel>(
                h => OnUpdated += h,
                h => OnUpdated -= h);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public IObservable<Unit> SetupAsync() {
            return Observable.Defer(() => {
                    var scope = new DisposableScope();
                    return _coroutineRunner.StartCoroutineAsync(SetupRoutine(scope), () => { scope.Dispose(); });
                })
                .Do(_ => OnUpdated?.Invoke(this));
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ITask.Update() {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            if (PlayerModel != null) {
                BattlePlayerModel.Delete(PlayerModel.Id);
                PlayerModel = null;
            }
        }

        /// <summary>
        /// 初期化コルーチン
        /// </summary>
        private IEnumerator SetupRoutine(IScope scope) {
            // PlayerModel生成
            PlayerModel = BattlePlayerModel.Create();

            // 必要なアセット読み込み
            var masterData = default(BattlePlayerMasterData);
            yield return new BattlePlayerMasterDataAssetRequest("pl001")
                .LoadAsync(scope)
                .Do(x => masterData = x)
                .StartAsEnumerator(scope);
            PlayerModel.Update(masterData.name, masterData.assetKey, masterData.healthMax);

            var setupData = default(PlayerActorSetupData);
            yield return new PlayerActorSetupDataAssetRequest(masterData.playerActorSetupDataId)
                .LoadAsync(scope)
                .Do(x => setupData = x)
                .StartAsEnumerator(scope);

            var actionDataList = new PlayerActorActionData[masterData.playerActorActionDataIds.Length];
            yield return masterData.playerActorActionDataIds
                .Select((x, i) => new PlayerActorActionDataAssetRequest(x)
                    .LoadAsync(scope)
                    .Do(y => actionDataList[i] = y))
                .WhenAll()
                .StartAsEnumerator(scope);

            PlayerModel.ActorModel.Update(setupData, actionDataList);
        }
        
        private BattleModel(object empty) : base(empty) {}
    }
}