using System;
using System.Collections;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using GameFramework.EntitySystems;
using GameFramework.MotionSystems;
using UniRx;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// プレイヤー制御用アクター
    /// </summary>
    public class PlayerActor : Actor, IStatusEventListener {
        /// <summary>
        /// 初期化データ
        /// </summary>
        public interface ISetupData {
            RuntimeAnimatorController Controller { get; }
            float AngularVelocity { get; }
        }

        /// <summary>
        /// アクション情報
        /// </summary>
        public struct ActionContext {
            public RuntimeAnimatorController controller;
        }

        // 行動キャンセル通知用
        private DisposableScope _actionScope = new DisposableScope();
        // コルーチン実行用
        private CoroutineRunner _coroutineRunner;
        // AnimatorController制御用
        private AnimatorControllerMotionPlayableProvider _animatorControllerProvider;
        // 現在のステータス名
        private string _currentStatus = "";
        // ステータスサイクル変化通知
        private Subject<Tuple<string, int>> _statusCycleSubject = new Subject<Tuple<string, int>>();
        
        // 移動制御用
        private MoveController _moveController;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerActor(Body body, ISetupData setupData)
            : base(body, 1) {
            var motionController = body.GetController<MotionController>();
            _animatorControllerProvider = motionController.Player.SetMotion(setupData.Controller, 0.0f);
            _coroutineRunner = new CoroutineRunner();
            _moveController = new MoveController(body.Transform, setupData.AngularVelocity, rate => {
                _animatorControllerProvider.GetPlayable().SetFloat("speed", rate);
            });
        }

        /// <summary>
        /// 死亡状態の設定
        /// </summary>
        public void SetDeath(bool death) {
            CancelAction();
            _animatorControllerProvider.GetPlayable().SetBool("death", death);
        }

        /// <summary>
        /// 指定方向への移動
        /// </summary>
        public void Move(Vector3 direction) {
            _moveController.Move(direction);
        }

        /// <summary>
        /// ジャンプ
        /// </summary>
        public void Jump() {
            if (_currentStatus != "Idle" && _currentStatus != "Run") {
                return;
            }
            _animatorControllerProvider.GetPlayable().SetTrigger("onJump");
        }
        
        /// <summary>
        /// アクションの再生
        /// </summary>
        public IObservable<Unit> PlayActionAsync(ActionContext context) {
            return Observable.Create<Unit>(observer => {
                CancelAction();
                
                return _coroutineRunner.StartCoroutineAsync(PlayActionRoutine(context, _actionScope),
                        () => {
                            // 状態を戻す
                            var motionController = Body.GetController<MotionController>();
                            motionController.Player.SetMotion(_animatorControllerProvider, 0.2f);
                        })
                    .Subscribe(_ => {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .ScopeTo(_actionScope);
            });
        }

        /// <summary>
        /// ダメージの再生
        /// </summary>
        public IObservable<Unit> DamageAsync(int damageIndex) {
            return Observable.Create<Unit>(observer => {
                CancelAction();
                
                return _coroutineRunner.StartCoroutineAsync(PlayDamageRoutine(damageIndex))
                    .Subscribe(_ => {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .ScopeTo(_actionScope);
            });
        }

        /// <summary>
        /// 行動キャンセル
        /// </summary>
        public void CancelAction() {
            _actionScope.Clear();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            _moveController.Update(Body.LayeredTime.DeltaTime);
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _moveController = null;
            _coroutineRunner.Dispose();
            _moveController?.Dispose();
            _moveController = null;
        }

        /// <summary>
        /// アクションの再生
        /// </summary>
        private IEnumerator PlayActionRoutine(ActionContext context, IScope cancelScope) {
            if (context.controller == null) {
                Debug.LogWarning("Not found action controller.");
                yield break;
            }
            
            // アクション専用のControllerに変更
            var motionController = Body.GetController<MotionController>();
            motionController.Player.SetMotion(context.controller, 0.2f);
            
            // 最終アクションが終わるまで待つ
            yield return WaitCycleRoutine("LastAction", 1, cancelScope);
            
            // 元のControllerに戻す
            motionController.Player.SetMotion(_animatorControllerProvider, 0.2f);
        }

        /// <summary>
        /// ダメージの再生
        /// </summary>
        private IEnumerator PlayDamageRoutine(int damageIndex) {
            // ダメージモーション再生
            SetDamageTrigger(damageIndex);
            // 遷移を待つ
            yield return null;
            // 待機に戻るまで待つ
            yield return WaitStatusRoutine("Idle");
        }

        /// <summary>
        /// 走り状態の設定
        /// </summary>
        private void SetRunningStatus(bool running) {
            var playable = _animatorControllerProvider.GetPlayable();
            playable.SetBool("running", running);
        }

        /// <summary>
        /// ダメージトリガー発生
        /// </summary>
        private void SetDamageTrigger(int damageIndex) {
            var playable = _animatorControllerProvider.GetPlayable();
            playable.SetInteger("damageIndex", damageIndex);
            playable.SetTrigger("onDamage");
        }

        /// <summary>
        /// 特定ステータスになるまで待つ
        /// </summary>
        private IEnumerator WaitStatusRoutine(string statusName) {
            while (statusName != _currentStatus) {
                yield return null;
            }
        }

        /// <summary>
        /// 特定ステータスで特定サイクル数に到達するまで待つ
        /// </summary>
        private IEnumerator WaitCycleRoutine(string statusName, int cycleCount, IScope cancelScope) {
            yield return _statusCycleSubject
                .Where(x => x.Item1 == statusName && x.Item2 >= cycleCount)
                .First()
                .StartAsEnumerator(cancelScope);
        }

        /// <summary>
        /// ステータスに入った時
        /// </summary>
        void IStatusEventListener.OnStatusEnter(string statusName) {
            _currentStatus = statusName;
        }
        
        /// <summary>
        /// ステータスのループ回数変化時
        /// </summary>
        void IStatusEventListener.OnStatusCycle(string statusName, int cycle) {
            _statusCycleSubject.OnNext(new Tuple<string, int>(statusName, cycle));            
        }
        
        /// <summary>
        /// ステータスを抜けた時
        /// </summary>
        void IStatusEventListener.OnStatusExit(string statusName) {
            
        }
    }
}