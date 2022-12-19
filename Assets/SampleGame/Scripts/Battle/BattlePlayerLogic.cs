using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace SampleGame {
    /// <summary>
    /// プレイヤー制御用アクター
    /// </summary>
    public class BattlePlayerLogic : EntityLogic {
        private PlayerActor _actor;
        private BattlePlayerModel _model;
        private GimmickController _gimmickController;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerLogic(PlayerActor actor, BattlePlayerModel model) {
            _actor = actor;
            _model = model;

            _gimmickController = _actor.Body.GetController<GimmickController>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            var input = Services.Get<BattleInput>();

            // ダメージ再生
            _model.OnDamagedAsObservable()
                .TakeUntil(scope)
                .Subscribe(x => {
                    _actor.DamageAsync(Random.Range(0, 3))
                        .Subscribe()
                        .ScopeTo(scope);
                });

            // 死亡
            _model.OnDeadAsObservable()
                .TakeUntil(scope)
                .Subscribe(_ => { _actor.SetDeath(true); });

            // 攻撃
            var actions = _model.ActorModel.Actions;
            input.AttackSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _actor.PlayActionAsync(actions[Random.Range(0, actions.Length)])
                        .Subscribe()
                        .ScopeTo(scope);
                });

            // ジャンプ
            input.JumpSubject
                .TakeUntil(scope)
                .Subscribe(_ => { _actor.Jump(); });
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            var input = Services.Get<BattleInput>();
            var cameraController = Services.Get<CameraController>();
            var camera = cameraController.MainCamera;

            // 移動
            var moveVector = input.MoveVector;
            var forward = camera.transform.forward;
            forward.y = 0.0f;
            forward.Normalize();
            var right = forward;
            right.x = forward.z;
            right.z = -forward.x;
            var moveDirection = forward * moveVector.y + right * moveVector.x;
            _actor.Move(moveDirection);

            // テスト用にダメージ発生
            if (Keyboard.current.qKey.wasPressedThisFrame) {
                _model.AddDamage(1);
            }

            if (Keyboard.current.tKey.wasPressedThisFrame) {
                _gimmickController.GetAnimationGimmicks("Test").Resume();
            }
            if (Keyboard.current.yKey.wasPressedThisFrame) {
                _gimmickController.GetAnimationGimmicks("Test").Resume(true);
            }
            if (Keyboard.current.gKey.wasPressedThisFrame) {
                _gimmickController.GetActiveGimmicks("Sphere").Activate();
            }
            if (Keyboard.current.hKey.wasPressedThisFrame) {
                _gimmickController.GetActiveGimmicks("Sphere").Deactivate();
            }
        }
    }
}