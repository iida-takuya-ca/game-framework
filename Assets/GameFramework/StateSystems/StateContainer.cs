using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.StateSystems {
    /// <summary>
    /// 状態制御クラス
    /// </summary>
    public class StateContainer<TState, TKey> : IDisposable
        where TState : IState<TKey>
        where TKey : IComparable {
        // Stateリスト
        private Dictionary<TKey, TState> _states = new Dictionary<TKey, TState>();
        // State用のScope
        private DisposableScope _scope = new DisposableScope();

        // 状態変更通知(Prev > Next)
        public event Action<TKey, TKey> OnChangedState;

        // 無効キー
        public TKey InvalidKey { get; private set; }
        // 現在のステートキー
        public TKey CurrentKey { get; private set; }
        // 遷移予定のステートキー
        public TKey NextKey { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Cleanup();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(TKey invalidKey, params TState[] states) {
            Cleanup();

            InvalidKey = invalidKey;
            CurrentKey = invalidKey;
            NextKey = invalidKey;

            foreach (var state in states) {
                // 無効キーは登録しない
                if (state.Key.Equals(invalidKey)) {
                    continue;
                }

                _states[state.Key] = state;
            }
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Cleanup() {
            Change(InvalidKey, true);

            _states.Clear();
            InvalidKey = default;
            CurrentKey = default;
            NextKey = default;
        }

        /// <summary>
        /// Stateの変更
        /// </summary>
        /// <param name="key">Stateキー</param>
        /// <param name="immediate">即時変更するか</param>
        public void Change(TKey key, bool immediate = false) {
            if (key.Equals(NextKey)) {
                return;
            }

            // 遷移先登録
            NextKey = key;

            // 即時反映する場合、更新を実行
            if (immediate) {
                Update(0.0f);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update(float deltaTime) {
            var state = default(TState);

            // 遷移
            if (!NextKey.Equals(CurrentKey)) {
                if (_states.TryGetValue(CurrentKey, out state)) {
                    state.OnExit(NextKey);
                    _scope.Dispose();
                }

                var prevKey = CurrentKey;
                CurrentKey = NextKey;
                if (_states.TryGetValue(CurrentKey, out state)) {
                    state.OnEnter(prevKey, _scope);
                }

                OnChangedState?.Invoke(prevKey, CurrentKey);
            }

            // 更新
            if (_states.TryGetValue(CurrentKey, out state)) {
                state.OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Stateの検索
        /// </summary>
        public TState FindState(TKey key) {
            if (_states.TryGetValue(key, out var state)) {
                return state;
            }

            return default;
        }
    }
}