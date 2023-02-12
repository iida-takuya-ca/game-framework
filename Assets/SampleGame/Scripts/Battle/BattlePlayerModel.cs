using System;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// バトル用プレイヤーモデル
    /// </summary>
    public class BattlePlayerModel : AutoIdModel<BattlePlayerModel> {
        public event Action<Tuple<BattlePlayerModel, int>> OnUpdatedHealth;
        public event Action<Tuple<BattlePlayerModel, int>> OnDamaged;
        public event Action<BattlePlayerModel> OnUpdated;
        public event Action<BattlePlayerModel> OnDead;

        public string Name { get; private set; } = "";
        public string AssetKey { get; private set; } = "";
        public int Health { get; private set; } = 0;
        public int HealthMax { get; private set; } = 0;
        public bool IsDead => Health <= 0;

        public BattlePlayerActorModel ActorModel { get; private set; }

        public IObservable<Tuple<BattlePlayerModel, int>> OnUpdatedHealthAsObservable() {
            return Observable.FromEvent<Tuple<BattlePlayerModel, int>>(
                h => OnUpdatedHealth += h,
                h => OnUpdatedHealth -= h);
        }

        public IObservable<Tuple<BattlePlayerModel, int>> OnDamagedAsObservable() {
            return Observable.FromEvent<Tuple<BattlePlayerModel, int>>(
                h => OnDamaged += h,
                h => OnDamaged -= h);
        }

        public IObservable<BattlePlayerModel> OnUpdatedAsObservable() {
            return Observable.FromEvent<BattlePlayerModel>(
                h => OnUpdated += h,
                h => OnUpdated -= h);
        }

        public IObservable<BattlePlayerModel> OnDeadAsObservable() {
            return Observable.FromEvent<BattlePlayerModel>(
                h => OnDead += h,
                h => OnDead -= h);
        }

        /// <summary>
        /// 値の更新
        /// </summary>
        public void Update(string name, string assetKey, int healthMax) {
            Name = name;
            AssetKey = assetKey;
            HealthMax = healthMax;
            Health = HealthMax;

            OnUpdatedHealth?.Invoke(new Tuple<BattlePlayerModel, int>(this, Health));
            OnUpdated?.Invoke(this);
        }

        /// <summary>
        /// ダメージの追加
        /// </summary>
        public void AddDamage(int damage) {
            if (IsDead) {
                return;
            }

            var newHealth = Mathf.Clamp(Health - damage, 0, HealthMax);
            damage = Health - newHealth;
            Health = newHealth;
            OnUpdatedHealth?.Invoke(new Tuple<BattlePlayerModel, int>(this, Health));
            OnDamaged?.Invoke(new Tuple<BattlePlayerModel, int>(this, damage));

            if (IsDead) {
                OnDead?.Invoke(this);
            }
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            BattlePlayerActorModel.Delete(ActorModel.Id);
            ActorModel = null;
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattlePlayerModel(int id) : base(id) {
            ActorModel = BattlePlayerActorModel.Create();
        }
    }
}