using GameFramework.Core;
using GameFramework.EntitySystems;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SampleGame {
    /// <summary>
    /// プレイヤー制御用アクター
    /// </summary>
    public class BattlePlayerLogic : EntityLogic {
        private PlayerActor _actor;
        private BattlePlayerModel _model;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerLogic(PlayerActor actor, BattlePlayerModel model) {
            _actor = actor;
            _model = model;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            // ダメージ再生
            _model.OnDamagedAsObservable()
                .TakeUntil(scope)
                .Subscribe(x => {
                    _actor.DamageAsync(Random.Range(0, 3))
                        .Subscribe();
                });
            
            // 移動
            
            // 攻撃
        }

        protected override void UpdateInternal() {
            // テストコード
            if (Input.GetKeyDown(KeyCode.D)) {
                _actor.DamageAsync(0)
                    .Subscribe();
            }
            if (Input.GetKey(KeyCode.UpArrow)) {
                _actor.ApproachAsync(_actor.Body.Transform.TransformPoint(Vector3.forward * 5))
                    .Subscribe();
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                _actor.ApproachAsync(_actor.Body.Transform.TransformPoint(-Vector3.forward * 5))
                    .Subscribe();
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                _actor.ApproachAsync(_actor.Body.Transform.TransformPoint(Vector3.right * 5))
                    .Subscribe();
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                _actor.ApproachAsync(_actor.Body.Transform.TransformPoint(-Vector3.right * 5))
                    .Subscribe();
            }
        }
    }
}
