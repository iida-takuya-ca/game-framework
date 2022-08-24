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
        
        // コルーチン実行用
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();
        
        // 読み込みスコープ
        private DisposableScope _loadScope;
        // 初期化スコープ
        private DisposableScope _setupScope;
        // アニメーションスコープ
        private DisposableScope _animationScope;
        
        // 親のSituation
        public Situation Parent => ParentContainer?.Owner;
        // 登録されているContainer
        public SituationContainer ParentContainer { get; private set; }
        // 自身の所持するContainer
        public SituationContainer Container { get; private set; }
        // インスタンス管理用
        public IServiceLocator ServiceLocator { get; private set; }
        // 現在状態
        public State CurrentState { get; private set; } = State.Invalid;

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            if (CurrentState == State.Invalid) {
                return;
            }
            
            // コルーチン更新
            _coroutineRunner.Update();

            // Active中はInterfaceのUpdateを呼ぶ
            if (CurrentState == State.Active) {
                ((ISituation)this).Update();
            }
            
            // コンテナの更新
            if (Container != null) {
                Container.Update();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            if (CurrentState == State.Invalid) {
                return;
            }

            // Active中はInterfaceのLateUpdateを呼ぶ
            if (CurrentState == State.Active) {
                ((ISituation)this).LateUpdate();
            }
            
            // コンテナの更新
            if (Container != null) {
                Container.LateUpdate();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Situation() {
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        void ISituation.Standby(SituationContainer container) {
            CurrentState = State.Standby;
            ParentContainer = container;
            // Containerの生成
            Container = CreateContainer();
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
        void ISituation.Release(SituationContainer container) {
            if (container != ParentContainer) {
                Debug.LogError("Invalid release parent.");
                return;
            }

            var info = new SituationContainer.TransitionInfo {
                container = container,
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
            
            ReleaseInternal(container);
            
            ParentContainer = null;
            Container.Dispose();
            ServiceLocator.Dispose();
            CurrentState = State.Invalid;
        }

        /// <summary>
        /// 登録解除処理
        /// </summary>
        /// <param name="parent">登録されていたContainer</param>
        protected virtual void ReleaseInternal(SituationContainer parent) {
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

        /// <summary>
        /// 自身が所持するコンテナの生成
        /// </summary>
        protected virtual SituationContainer CreateContainer() {
            return new SituationContainer();
        }
    }
}