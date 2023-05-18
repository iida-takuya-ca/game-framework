using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.ProjectileSystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.VfxSystems;
using SampleGame.SequenceEvents;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace SampleGame.Battle {
    /// <summary>
    /// プレイヤー制御用Presenter
    /// </summary>
    public class BattlePlayerPresenter : EntityLogic {
        private PlayerActor _actor;
        private BattlePlayerModel _model;
        private GimmickController _gimmickController;
        private VfxManager.Handle _auraVfxHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerPresenter(PlayerActor actor, BattlePlayerModel model) {
            _actor = actor;
            _model = model;

            _gimmickController = _actor.Body.GetController<GimmickController>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            var ct = scope.ToCancellationToken();
            var input = Services.Get<BattleInput>();
            var vfxManager = Services.Get<VfxManager>();
            
            var body = _actor.Body;
            var context = new VfxManager.Context {
                prefab = _actor.Data.AuraPrefab,
                constraintPosition = true,
                constraintRotation = true,
                localScale = Vector3.one,
            };
            _auraVfxHandle = vfxManager.Get(context, body.Transform, body.Transform, body.LayeredTime);
            _auraVfxHandle.ScopeTo(scope);
            
            // SequenceEvent登録
            BindSequenceEventHandlers(scope);

            //-- View反映系
            // 攻撃
            _model.OnAttackEventAsObservable()
                .TakeUntil(scope)
                .Subscribe(x => {
                    _actor.PlayGeneralActionAsync(x.Item2, ct).Forget();
                });
            
            // ダメージ再生
            _model.OnDamagedAsObservable()
                .TakeUntil(scope)
                .Subscribe(x => {
                    _actor.DamageAsync(Random.Range(0, 3), ct).Forget();
                });

            // 死亡
            _model.OnDeadAsObservable()
                .TakeUntil(scope)
                .Subscribe(_ => { _actor.SetDeath(true); });

            //-- 入力系
            // 攻撃
            var actions = _model.ActorModel.GeneralActions;
            input.AttackSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _model.GeneralAction(Random.Range(0, actions.Length));
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
            var cameraManager = Services.Get<CameraManager>();
            var camera = cameraManager.OutputCamera;
            var projectileObjectManager = Services.Get<ProjectileObjectManager>();

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
            
            // テスト用アクション入力
            for (var i = 0; i < 9; i++) {
                if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame) {
                    _model.GeneralAction(i);
                }
            }

            // テスト用にダメージ発生
            if (Keyboard.current.qKey.wasPressedThisFrame) {
                _model.AddDamage(1);
            }

            if (Keyboard.current.vKey.wasPressedThisFrame) {
                _actor.Vibrate();
            }

            if (Keyboard.current.zKey.wasPressedThisFrame) {
                _actor.Body.IsVisible ^= true;
            }

            // テスト用にTimeScale変更
            if (Keyboard.current.lKey.isPressed) {
                _actor.Body.LayeredTime.LocalTimeScale = 0.25f;
            }
            else {
                _actor.Body.LayeredTime.LocalTimeScale = 1.0f;
            }

            // テスト用にギミック再生
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

            if (Keyboard.current.fKey.wasPressedThisFrame) {
                _gimmickController.GetAnimationGimmicks("Damage").Play();
            }

            // テスト用に遠距離攻撃
            if (Keyboard.current.pKey.wasPressedThisFrame) {
                var baseTrans = _actor.Body.Locators["Head"];
                var startPos = baseTrans.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
                var targetPos = Vector3.zero;
                var velocity = -baseTrans.right * 5;
                var projectile = new HomingProjectile(startPos, targetPos, velocity, 1.0f, 0.2f, 50.0f, 50.0f);
                projectileObjectManager.Play(_actor.Data.BulletPrefab, projectile, LayerMask.GetMask("Default"), 1,
                    null, null,
                    result => { Debug.Log($"Hit:[{result.hitCount}]{result.raycastHit.point}"); });
            }
            
            // テスト用にループエフェクト
            if (Keyboard.current.oKey.isPressed) {
                _auraVfxHandle.ContextPosition += Vector3.right * _actor.Body.DeltaTime;
                if (!_auraVfxHandle.IsPlaying) {
                    _auraVfxHandle.Play();
                }
            }
            else {
                if (_auraVfxHandle.IsPlaying) {
                    _auraVfxHandle.Stop();
                }
            }
        }

        /// <summary>
        /// SequenceEventに関する処理登録
        /// </summary>
        private void BindSequenceEventHandlers(IScope scope) {
            var sequenceController = _actor.SequenceController;
            var vfxManager = Services.Get<VfxManager>();
            var cameraManager = Services.Get<CameraManager>();
            
            sequenceController.BindSignalEventHandler<BodyEffectSingleEvent, BodyEffectSingleEventHandler>(handler => {
                handler.Setup(vfxManager, _actor.Body);
            });
            sequenceController.BindRangeEventHandler<BodyEffectRangeEvent, BodyEffectRangeEventHandler>(handler => {
                handler.Setup(vfxManager, _actor.Body);
            });
            sequenceController.BindRangeEventHandler<CameraRangeEvent, CameraRangeEventHandler>(handler => {
                handler.Setup(cameraManager);
            });
            sequenceController.BindRangeEventHandler<MotionCameraRangeEvent, MotionCameraRangeEventHandler>(handler => {
                handler.Setup(cameraManager, _actor.Body.Transform, _actor.Body.LayeredTime);
            });
            sequenceController.BindRangeEventHandler<LookAtMotionCameraRangeEvent, LookAtMotionCameraRangeEventHandler>(handler => {
                handler.Setup(cameraManager, _actor.Body.Transform, _actor.Body.LayeredTime);
            });

            scope.OnExpired += () => sequenceController.ResetEventHandlers();
        }
    }
}