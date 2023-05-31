using System;
using System.Collections;
using System.Threading;
using ActionSequencer;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using UniRx;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// プレイヤー制御用アクター
    /// </summary>
    public class PlayerActor : Actor {
        /// <summary>
        /// 初期化データ
        /// </summary>
        public interface ISetupData {
            RuntimeAnimatorController Controller { get; }
            AnimationClip VibrateClip { get; }
            float AngularVelocity { get; }
            GameObject BulletPrefab { get; }
            GameObject AuraPrefab { get; }
        }

        /// <summary>
        /// アクションデータ
        /// </summary>
        public interface IActionData {
            RuntimeAnimatorController Controller { get; }
            SequenceClip SequenceClip { get; }
        }

        // 行動キャンセル通知用
        private DisposableScope _actionScope = new DisposableScope();
        // コルーチン実行用
        private CoroutineRunner _coroutineRunner;
        // シーケンス再生用
        private SequenceController _sequenceController;
        // AnimatorController制御用
        private AnimatorControllerPlayableProvider _basePlayableProvider;
        // 現在のステータス名
        private string _currentStatus = "";
        // ステータスリスナー
        private StatusEventListener _statusEventListener;
        // モーション制御クラス
        private MotionController _motionController;

        // 移動制御用
        private MoveController _moveController;
        
        // データ
        public ISetupData Data { get; private set; }
        // 外部参照用のSequenceController
        public IReadOnlySequenceController SequenceController => _sequenceController;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerActor(Body body, ISetupData setupData)
            : base(body) {
            Data = setupData;
            _statusEventListener = body.GetComponent<StatusEventListener>();
            _motionController = body.GetController<MotionController>();
            _coroutineRunner = new CoroutineRunner();
            _sequenceController = new SequenceController();
            _moveController = new MoveController(body.Transform, setupData.AngularVelocity,
                rate => { _basePlayableProvider.GetPlayable().SetFloat("speed", rate); });
        }

        /// <summary>
        /// 死亡状態の設定
        /// </summary>
        public void SetDeath(bool death) {
            CancelAction();
            _basePlayableProvider.GetPlayable().SetBool("death", death);
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

            _basePlayableProvider.GetPlayable().SetTrigger("onJump");
        }

        /// <summary>
        /// 汎用アクションの再生
        /// </summary>
        public async UniTask PlayGeneralActionAsync(IActionData actionData, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            await PlayActionAsync(PlayActionRoutine(actionData, _actionScope), () => {
                if (Body.IsValid) {
                    // 状態を戻す
                    var motionController = Body.GetController<MotionController>();
                    motionController.Player.Change(_basePlayableProvider, 0.2f);
                }
            }, ct);
        }

        /// <summary>
        /// ダメージの再生
        /// </summary>
        public async UniTask DamageAsync(int damageIndex, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            await PlayActionAsync(PlayDamageRoutine(damageIndex), null, ct);
        }

        /// <summary>
        /// 振動の再生
        /// </summary>
        public void Vibrate() {
            _motionController.Player.Change(1, new AnimationClipPlayableProvider(Data.VibrateClip, true), 0.2f);
        }

        /// <summary>
        /// 行動キャンセル
        /// </summary>
        public void CancelAction() {
            _actionScope.Clear();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            // 基本モーションの設定
            _basePlayableProvider = _motionController.Player.Change(Data.Controller, 0.0f, false);
            
            // 加算モーションレイヤーの追加
            _motionController.Player.BuildAdditionalLayers(new [] {
                new MotionPlayer.LayerSettings {
                    additive = true
                }
            });
            
            _statusEventListener.EnterSubject
                .TakeUntil(scope)
                .Subscribe(x => { _currentStatus = x; });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            _motionController.Player.ResetAdditionalLayers();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            var deltaTime = Body.LayeredTime.DeltaTime;
            
            _coroutineRunner.Update();
            _moveController.Update(deltaTime);
            _sequenceController.Update(deltaTime);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            CancelAction();
            
            _coroutineRunner.Dispose();
            _moveController.Dispose();
            _sequenceController.Dispose();
            _coroutineRunner = null;
            _moveController = null;
            _sequenceController = null;
        }

        /// <summary>
        /// アクションの再生
        /// </summary>
        private async UniTask PlayActionAsync(IEnumerator enumerator, Action onCancel, CancellationToken ct) {
            CancelAction();
            
            var coroutine = _coroutineRunner.StartCoroutine(enumerator);
            _actionScope.OnExpired += () => {
                _coroutineRunner.StopCoroutine(coroutine);
                onCancel?.Invoke();
            };

            ct.Register(CancelAction);

            await UniTask.WaitUntil(() => coroutine.IsDone, cancellationToken: ct);
        }

        /// <summary>
        /// アクションの再生
        /// </summary>
        private IEnumerator PlayActionRoutine(IActionData actionData, IScope cancelScope) {
            if (actionData.Controller == null) {
                Debug.LogWarning("Not found action controller.");
                yield break;
            }

            // アクション専用のControllerに変更
            var motionController = Body.GetController<MotionController>();
            motionController.Player.Change(actionData.Controller, 0.2f);
            
            // Sequence再生
            var sequenceHandle = default(SequenceHandle);
            if (actionData.SequenceClip != null) {
                sequenceHandle = _sequenceController.Play(actionData.SequenceClip);
            }

            // 最終アクションが終わるまで待つ
            yield return WaitCycleRoutine("LastAction", 1, cancelScope);

            // 元のControllerに戻す
            motionController.Player.Change(_basePlayableProvider, 0.2f);
            
            // Sequence完了を待つ
            while (!sequenceHandle.IsDone) {
                yield return null;
            }
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
        /// ダメージトリガー発生
        /// </summary>
        private void SetDamageTrigger(int damageIndex) {
            var playable = _basePlayableProvider.GetPlayable();
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
            yield return _statusEventListener.CycleSubject
                .Where(x => x.Item1 == statusName && x.Item2 >= cycleCount)
                .First()
                .StartAsEnumerator(cancelScope);
        }
    }
}