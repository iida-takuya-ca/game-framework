using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// コルーチン本体
    /// </summary>
    public class Coroutine : IEnumerator {
        private IEnumerator _enumerator;
        private Stack<object> _stack;

        // 現在の処理位置
        public object Current { get; private set; }

        // 完了しているか
        public bool IsDone { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="enumerator">処理</param>
        public Coroutine(IEnumerator enumerator) {
            _enumerator = enumerator;
            _stack = new Stack<object>();
            Reset();
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        public void Reset() {
            _stack.Clear();
            _stack.Push(_enumerator);
            Current = null;
            IsDone = false;
        }

        /// <summary>
        /// コルーチン進行
        /// </summary>
        /// <returns>次の処理があるか？</returns>
        public bool MoveNext() {
            Update();
            return !IsDone;
        }

        /// <summary>
        /// 処理更新
        /// </summary>
        private void Update() {
            // 完了処理
            void Done() {
                _stack.Clear();
                Current = null;
                IsDone = true;
            }

            // スタックがなければ、完了
            if (_stack.Count == 0) {
                Done();
                return;
            }

            // スタックを取り出して、処理を進める
            var peek = _stack.Peek();
            Current = peek;

            if (peek == null) {
                _stack.Pop();
            }
            else if (peek is IEnumerator enumerator) {
                if (enumerator.MoveNext()) {
                    _stack.Push(enumerator.Current);
                }
                else {
                    _stack.Pop();
                }
                Update();
            }
            else if (peek is IEnumerable enumerable) {
                _stack.Pop();
                _stack.Push(enumerable.GetEnumerator());
                Update();
            }
            else if (peek is AsyncOperation asyncOperation) {
                if (asyncOperation.isDone) {
                    _stack.Pop();
                    Update();
                }
            }
            else if (peek is WaitForSeconds waitForSeconds) {
                _stack.Pop();
                _stack.Push(UnwrapWaitForSeconds(waitForSeconds));
                Update();
            }
            else {
                throw new NotSupportedException($"{peek.GetType()} is not supported.");
            }
        }

        /// <summary>
        /// WaitForSecondsをIEnumeratorに変換
        /// </summary>
        private IEnumerator UnwrapWaitForSeconds(WaitForSeconds waitForSeconds) {
            var accessor = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
            var second = (float)accessor.GetValue(waitForSeconds);
            var startTime = DateTimeOffset.UtcNow;
            while (true) {
                yield return null;

                var elapsed = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
                if (elapsed >= second) {
                    break;
                }
            }
        }
    }
}