using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション
    /// </summary>
    public abstract class Situation : ISituation {
        // 状態
        public enum State {
            Invalid = -1,
            Standby, // 待機状態
            Loaded, // 読み込み済
            SetupFinished, // 初期化済
            Active, // アクティブ
        }
        
        // 遷移情報
        public class TransitionInfo {
            public ISituation owner;
            public ISituation prev;
            public ISituation next;
            public TransitionState state;
            public bool back;
            public ITransitionEffect[] effects = new ITransitionEffect[0];
        }

        // 子シチュエーションスタック
        private List<Situation> _childStack = new List<Situation>();
        // 遷移中情報
        private TransitionInfo _transitionInfo;
        // コルーチン実行用
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();
        
        // 読み込みスコープ
        private DisposableScope _loadScope;
        // 初期化スコープ
        private DisposableScope _setupScope;
        // アニメーションスコープ
        private DisposableScope _animationScope;

        // ルートシチュエーションか
        public bool IsRoot => Parent == null && CurrentState != State.Invalid;
        // 親シチュエーション
        public Situation Parent { get; private set; }        
        // インスタンス管理用
        public IServiceLocator ServiceLocator { get; private set; }
        // 現在状態
        public State CurrentState { get; private set; } = State.Invalid;
        // 現在の子シチュエーション
        public Situation CurrentChild => _childStack.Count > 0 ? _childStack[_childStack.Count - 1] : null;

        /// <summary>
        /// ルートとしてセットアップ
        /// </summary>
        public TransitionHandle SetupRoot() {
            if (Parent != null) {
                return new TransitionHandle(new Exception("Child situation is not root."));
            }

            if (CurrentState != State.Invalid) {
                return new TransitionHandle(new Exception($"Already setup root."));
            }
            
            var self = (ISituation)this;
            var transitionInfo = new TransitionInfo {
                owner = this,
                prev = null,
                next = this,
                back = false,
                state = TransitionState.Standby
            };
            var handle = new TransitionHandle(transitionInfo);
            
            IEnumerator SetupRoutine() {
                // 初期化開始
                transitionInfo.state = TransitionState.Initializing;

                // 読み込み処理
                yield return self.LoadRoutine(handle);
            
                // 初期化処理
                self.Setup(handle);

                // オープン開始
                transitionInfo.state = TransitionState.Opening;
            
                // 開く処理
                yield return self.OpenRoutine(handle);

                transitionInfo.state = TransitionState.Completed;
            }

            // Rootとして初期化を実行
            self.Standby(null);
            _coroutineRunner.StartCoroutine(SetupRoutine());

            return handle;
        }

        /// <summary>
        /// ルートの解放処理
        /// </summary>
        public void CleanupRoot() {
            if (!IsRoot) {
                return;
            }

            var self = (ISituation)this;
            self.Release(null);
        }
        
        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="situation">遷移先のシチュエーション(nullの場合、全部閉じる)</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(Situation situation, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            var nextName = situation != null ? situation.GetType().Name : "null";
            
            if (_transitionInfo != null) {
                return new TransitionHandle(new Exception($"In transit other. Situation:{nextName}"));
            }
            
            // 既に同タイプのシチュエーションが登録されている場合、そこにスタックを戻す
            var backIndex = -1;
            var back = false;

            if (situation != null) {
                for (var i = 0; i < _childStack.Count; i++) {
                    // 同じインスタンスは使いまわす
                    if (_childStack[i] == situation) {
                        backIndex = i;
                        back = true;
                        break;
                    }

                    // 同じ型は置き換える
                    if (_childStack[i].GetType() == situation.GetType()) {
                        // 遷移の必要なければキャンセル
                        if (i == _childStack.Count - 1) {
                            return new TransitionHandle(new Exception($"Cancel transit. Situation:{nextName}"));
                        }
                    
                        var old = _childStack[i];
                        ((ISituation)old).Release(this);
                        _childStack[i] = situation;
                        backIndex = i;
                        back = true;
                    }
                }
            }
            else {
                back = true;
            }

            var prev = (ISituation)(_childStack.Count > 0 ? _childStack[_childStack.Count - 1] : null);
            var next = (ISituation)situation;

            // 遷移の必要がなければキャンセル扱い
            if (prev == next) {
                return new TransitionHandle(new Exception($"Cancel transit. Situation:{nextName}"));
            }
            
            // 遷移情報の取得
            var transition = overrideTransition ?? GetDefaultTransition();
            
            // 遷移可能チェック
            if (!CheckTransition(next, transition)) {
                return new TransitionHandle(
                    new Exception($"Cant transition. Situation:{nextName} Transition:{transition}"));
            }
            
            // 戻る場合
            if (back) {
                // 現在のSituationをStackから除外
                _childStack.RemoveAt(_childStack.Count - 1);

                // 戻り先までの間にあるSituationをリリースして、Stackクリア
                for (var i = _childStack.Count - 1; i > backIndex; i--) {
                    ((ISituation)_childStack[i]).Release(this);
                    _childStack.RemoveAt(i);
                }
            }
            // 進む場合
            else {
                // スタックに登録
                _childStack.Add(situation);
            }

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                owner = this,
                back = back,
                prev = prev,
                next = next,
                state = TransitionState.Standby,
                effects = effects
            };
            
            // コルーチンの登録
            _coroutineRunner.StartCoroutine(transition.TransitRoutine(_transitionInfo), () => {
                // 戻る時はここでRelease
                if (back && prev != null) {
                    prev.Release(this);
                }
                _transitionInfo = null;
            });
            
            // スタンバイ状態
            _transitionInfo.next?.Standby(this);
            
            // ハンドルの返却
            return new TransitionHandle(_transitionInfo);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        public TransitionHandle Back(ITransition overrideTransition = null) {
            if (_childStack.Count <= 0) {
                return new TransitionHandle(new Exception("Not found stack."));
            }

            var next = _childStack.Count > 1 ? _childStack[_childStack.Count - 2] : null;
            return Transition(next, overrideTransition);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            if (CurrentState == State.Invalid) {
                return;
            }

            if (Parent != null) {
                return;
            }
            
            UpdateRecursive();
        }
        private void UpdateRecursive() {
            // コルーチン更新
            _coroutineRunner.Update();
            
            // 子の再帰更新
            if (CurrentChild != null) {
                CurrentChild.UpdateRecursive();
            }

            // 遷移終わったらインターフェース更新
            if (_transitionInfo == null) {
                var current = (ISituation)CurrentChild;
                if (current != null) {
                    current.Update();
                }
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            if (CurrentState == State.Invalid) {
                return;
            }

            if (Parent != null) {
                return;
            }
            
            UpdateRecursive();
        }
        private void LateUpdateRecursive() {
            // 子の更新
            if (CurrentChild != null) {
                CurrentChild.LateUpdateRecursive();
            }

            // 遷移終わったらインターフェース更新
            if (_transitionInfo == null) {
                var current = (ISituation)CurrentChild;
                if (current != null) {
                    current.LateUpdate();
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Situation() {
        }

        /// <summary>
        /// デフォルトの遷移を取得
        /// </summary>
        protected virtual ITransition GetDefaultTransition() {
            return new OutInTransition();
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        void ISituation.Standby(ISituation parent) {
            CurrentState = State.Standby;
            Parent = (Situation)parent;
            // ServiceLocatorの生成
            ServiceLocator = new ServiceLocator(Parent?.ServiceLocator ?? ProjectServiceLocator.Instance);
            StandbyInternal(Parent);
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        /// <param name="parent">親シチュエーション</param>
        protected virtual void StandbyInternal(Situation parent) {
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        IEnumerator ISituation.LoadRoutine(TransitionHandle handle) {
            _loadScope = new DisposableScope();
            yield return LoadRoutineInternal(handle, _loadScope);
            CurrentState = State.Loaded;
        }

        /// <summary>
        /// 読み込み処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">読み込み用スコープ(LoadRoutine～Unloadまで)</param>
        protected virtual IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ISituation.Setup(TransitionHandle handle) {
            _setupScope = new DisposableScope();
            SetupInternal(handle, _setupScope);
            CurrentState = State.SetupFinished;
        }

        /// <summary>
        /// 初期化処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">読み込み用スコープ(Setup～Cleanupまで)</param>
        protected virtual void SetupInternal(TransitionHandle handle, IScope scope) {
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        IEnumerator ISituation.OpenRoutine(TransitionHandle handle) {
            _animationScope = new DisposableScope();
            yield return OpenRoutineInternal(handle, _animationScope);
            CurrentState = State.Active;
            _animationScope.Dispose();
        }

        /// <summary>
        /// 開く処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="animationScope">アニメーションキャンセル用スコープ(OpenRoutine中)</param>
        protected virtual IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield break;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ISituation.Update() {
            UpdateInternal();
        }

        /// <summary>
        /// 更新処理(内部用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        void ISituation.LateUpdate() {
            LateUpdateInternal();
        }

        /// <summary>
        /// 後更新処理(内部用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        IEnumerator ISituation.CloseRoutine(TransitionHandle handle) {
            _animationScope = new DisposableScope();
            CurrentState = State.SetupFinished;
            yield return CloseRoutineInternal(handle, _animationScope);
            _animationScope.Dispose();
        }

        /// <summary>
        /// 閉じる処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="animationScope">アニメーションキャンセル用スコープ(OpenRoutine中)</param>
        protected virtual IEnumerator CloseRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield break;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        void ISituation.Cleanup(TransitionHandle handle) {
            CurrentState = State.Loaded;
            CleanupInternal(handle);
            _setupScope.Dispose();
        }

        /// <summary>
        /// 初期化処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void CleanupInternal(TransitionHandle handle) {
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        void ISituation.Unload(TransitionHandle handle) {
            CurrentState = State.Standby;
            UnloadInternal(handle);
            _loadScope.Dispose();
        }

        /// <summary>
        /// 解放処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void UnloadInternal(TransitionHandle handle) {
        }

        /// <summary>
        /// 登録解除処理
        /// </summary>
        void ISituation.Release(ISituation parent) {
            if (parent != Parent) {
                Debug.LogError("Invalid release parent.");
                return;
            }

            var info = new TransitionInfo {
                owner = parent,
                prev = this,
                next = null,
                back = false,
                state = TransitionState.Canceled
            };
            var handle = new TransitionHandle(info);

            var situation = (ISituation)this;
            if (CurrentState == State.Active || CurrentState == State.SetupFinished) {
                situation.Cleanup(handle);
            }
            if (CurrentState == State.Loaded) {
                situation.Unload(handle);
            }
            
            ReleaseInternal((Situation)parent);
            
            Parent = null;
            ServiceLocator.Dispose();
            CurrentState = State.Invalid;
        }

        /// <summary>
        /// 登録解除処理
        /// </summary>
        /// <param name="parent">親シチュエーション</param>
        protected virtual void ReleaseInternal(Situation parent) {
        }

        /// <summary>
        /// 遷移チェック
        /// </summary>
        private bool CheckTransition(ISituation childSituation, ITransition transition) {
            if (transition == null) {
                return false;
            }

            // null遷移は常に許可
            if (childSituation == null) {
                return true;
            }

            return CheckTransitionInternal((Situation)childSituation, transition);
        }

        /// <summary>
        /// 遷移チェック
        /// </summary>
        /// <param name="childSituation">遷移するの子シチュエーション</param>
        /// <param name="transition">遷移処理</param>
        /// <returns>遷移可能か</returns>
        protected virtual bool CheckTransitionInternal(Situation childSituation, ITransition transition) {
            return true;
        }
    }
}