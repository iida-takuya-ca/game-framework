using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.CoroutineSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション管理用クラス
    /// </summary>
    public class SituationContainer : IDisposable {
        // 遷移情報
        public class TransitionInfo {
            public SituationContainer container;
            public ISituation prev;
            public ISituation next;
            public TransitionState state;
            public bool back;
            public ITransitionEffect[] effects = new ITransitionEffect[0];
        }

        // 子シチュエーションスタック
        private List<Situation> _stack = new List<Situation>();
        // 遷移中情報
        private TransitionInfo _transitionInfo;
        // コルーチン実行用
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();

        // 持ち主のSituation
        public Situation Owner { get; private set; }
        // 現在のシチュエーション
        public Situation Current => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;
        
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
                for (var i = 0; i < _stack.Count; i++) {
                    // 同じインスタンスは使いまわす
                    if (_stack[i] == situation) {
                        backIndex = i;
                        back = true;
                        break;
                    }

                    // 同じ型は置き換える
                    if (_stack[i].GetType() == situation.GetType()) {
                        // 遷移の必要なければキャンセル
                        if (i == _stack.Count - 1) {
                            return new TransitionHandle(new Exception($"Cancel transit. Situation:{nextName}"));
                        }
                    
                        var old = _stack[i];
                        ((ISituation)old).Release(this);
                        _stack[i] = situation;
                        backIndex = i;
                        back = true;
                    }
                }
            }
            else {
                back = true;
            }

            var prev = (ISituation)(_stack.Count > 0 ? _stack[_stack.Count - 1] : null);
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
                _stack.RemoveAt(_stack.Count - 1);

                // 戻り先までの間にあるSituationをリリースして、Stackクリア
                for (var i = _stack.Count - 1; i > backIndex; i--) {
                    ((ISituation)_stack[i]).Release(this);
                    _stack.RemoveAt(i);
                }
            }
            // 進む場合
            else {
                // スタックに登録
                _stack.Add(situation);
            }

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                container = this,
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
            if (_stack.Count <= 0) {
                return new TransitionHandle(new Exception("Not found stack."));
            }

            var next = _stack.Count > 1 ? _stack[_stack.Count - 2] : null;
            return Transition(next, overrideTransition);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            // コルーチン更新
            _coroutineRunner.Update();
            
            // 遷移中のシチュエーション更新
            if (_transitionInfo != null) {
                if (_transitionInfo.prev is Situation prev) {
                    prev.Update();
                }
                if (_transitionInfo.next is Situation next) {
                    next.Update();
                }
            }
            // カレントシチュエーションの更新
            else {
                var current = Current;
                if (current != null) {
                    current.Update();
                }
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            // 遷移中のシチュエーション更新
            if (_transitionInfo != null) {
                if (_transitionInfo.prev is Situation prev) {
                    prev.LateUpdate();
                }
                if (_transitionInfo.next is Situation next) {
                    next.LateUpdate();
                }
            }
            // カレントシチュエーションの更新
            else {
                var current = (ISituation)Current;
                if (current != null) {
                    current.LateUpdate();
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationContainer(Situation owner = null) {
            Owner = owner;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (Current != null) {
                ((ISituation)Current).Release(this);
            }
            _coroutineRunner.Dispose();
            _transitionInfo = null;
        }

        /// <summary>
        /// デフォルトの遷移を取得
        /// </summary>
        protected virtual ITransition GetDefaultTransition() {
            return new OutInTransition();
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