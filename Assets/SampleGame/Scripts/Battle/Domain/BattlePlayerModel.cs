using System;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用プレイヤーモデル
    /// </summary>
    public class BattlePlayerModel : AutoIdModel<BattlePlayerModel> {
        public event Action<Tuple<BattlePlayerModel, int>> OnUpdatedHealthEvent;
        public event Action<Tuple<BattlePlayerModel, int>> OnDamagedEvent;
        public event Action<BattlePlayerModel> OnUpdatedEvent;
        public event Action<BattlePlayerModel> OnDeadEvent;
        public event Action<(BattlePlayerModel, PlayerActor.IActionData, int)> OnAttackEvent;

        public string Name { get; private set; } = "";
        public string AssetKey { get; private set; } = "";
        public int Health { get; private set; } = 0;
        public int HealthMax { get; private set; } = 0;
        public bool IsDead => Health <= 0;

        public BattlePlayerActorModel ActorModel { get; private set; }

        public IObservable<Tuple<BattlePlayerModel, int>> OnUpdatedHealthAsObservable() {
            return Observable.FromEvent<Tuple<BattlePlayerModel, int>>(
                h => OnUpdatedHealthEvent += h,
                h => OnUpdatedHealthEvent -= h);
        }

        public IObservable<Tuple<BattlePlayerModel, int>> OnDamagedAsObservable() {
            return Observable.FromEvent<Tuple<BattlePlayerModel, int>>(
                h => OnDamagedEvent += h,
                h => OnDamagedEvent -= h);
        }

        public IObservable<BattlePlayerModel> OnUpdatedAsObservable() {
            return Observable.FromEvent<BattlePlayerModel>(
                h => OnUpdatedEvent += h,
                h => OnUpdatedEvent -= h);
        }

        public IObservable<BattlePlayerModel> OnDeadAsObservable() {
            return Observable.FromEvent<BattlePlayerModel>(
                h => OnDeadEvent += h,
                h => OnDeadEvent -= h);
        }

        public IObservable<(BattlePlayerModel, PlayerActor.IActionData, int)> OnAttackEventAsObservable() {
            return Observable.FromEvent<(BattlePlayerModel, PlayerActor.IActionData, int)>(
                h => OnAttackEvent += h,
                h => OnAttackEvent -= h);
        }

        /// <summary>
        /// 値の更新
        /// </summary>
        public void Update(string name, string assetKey, int healthMax) {
            Name = name;
            AssetKey = assetKey;
            HealthMax = healthMax;
            Health = HealthMax;

            OnUpdatedHealthEvent?.Invoke(new Tuple<BattlePlayerModel, int>(this, Health));
            OnUpdatedEvent?.Invoke(this);
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
            OnUpdatedHealthEvent?.Invoke(new Tuple<BattlePlayerModel, int>(this, Health));
            OnDamagedEvent?.Invoke(new Tuple<BattlePlayerModel, int>(this, damage));

            if (IsDead) {
                OnDeadEvent?.Invoke(this);
            }
        }

        /// <summary>
        /// 汎用アクション実行
        /// </summary>
        public void GeneralAction(int index) {
            if (IsDead) {
                return;
            }

            if (index < 0 || index >= ActorModel.GeneralActions.Length) {
                return;
            }
            
            OnAttackEvent?.Invoke((this, ActorModel.GeneralActions[index], index));
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