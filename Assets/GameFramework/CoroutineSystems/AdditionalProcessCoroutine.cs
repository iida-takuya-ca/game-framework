using System;
using System.Collections;

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// コルーチンに追加処理を行う機能
    /// </summary>
    public class AdditionalProcessCoroutine : IEnumerator {
        // ベースのコルーチン処理
        private IEnumerator _baseEnumerator;
        // 追記処理
        private Action _additionalFunc;

        // 現在の位置(未使用)
        public object Current => _baseEnumerator.Current;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="baseEnumerator">ベースのコルーチン処理</param>
        /// <param name="additionalFunc">追記処理</param>
        public AdditionalProcessCoroutine(IEnumerator baseEnumerator, Action additionalFunc) {
            _baseEnumerator = baseEnumerator;
            _additionalFunc = additionalFunc;
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        public void Reset() {
            if (_baseEnumerator != null) {
                _baseEnumerator.Reset();
            }
        }

        /// <summary>
        /// コルーチン進行
        /// </summary>
        /// <returns>次の処理があるか？</returns>
        public bool MoveNext() {
            if (_baseEnumerator.MoveNext()) {
                _additionalFunc?.Invoke();
                return true;
            }

            return false;
        }
    }
}