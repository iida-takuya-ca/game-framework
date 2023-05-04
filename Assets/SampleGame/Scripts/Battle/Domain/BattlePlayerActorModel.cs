using System;
using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// バトル用プレイヤーのアクター情報をまとめたモデル
    /// </summary>
    public class BattlePlayerActorModel : AutoIdModel<BattlePlayerActorModel> {
        public PlayerActorSetupData Setup { get; private set; }
        public PlayerActorActionData[] GeneralActions { get; private set; }

        public event Action<BattlePlayerActorModel> OnUpdated;

        public IObservable<BattlePlayerActorModel> OnUpdatedAsObservable() {
            return Observable.FromEvent<BattlePlayerActorModel>(
                h => OnUpdated += h,
                h => OnUpdated -= h);
        }

        /// <summary>
        /// 値の更新
        /// </summary>
        public void Update(PlayerActorSetupData setupData, PlayerActorActionData[] generalActionDataList) {
            Setup = setupData;
            GeneralActions = generalActionDataList;
            OnUpdated?.Invoke(this);
        }
        
        private BattlePlayerActorModel(int id) : base(id) {}
    }
}