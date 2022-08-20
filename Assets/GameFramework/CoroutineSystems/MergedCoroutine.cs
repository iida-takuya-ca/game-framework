using System.Linq;
using System.Collections;

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// 並列実行用コルーチン
    /// </summary>
    public class MergedCoroutine : IEnumerator {
        private IEnumerator[] _enumerators;

        // 完了しているか
        public bool IsDone { get; private set; }
        // 現在の位置(未使用)
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="enumerators">非同期処理リスト</param>
        public MergedCoroutine(params IEnumerator[] enumerators) {
            _enumerators = enumerators;
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        void IEnumerator.Reset() {
            for (var i = 0; i < _enumerators.Length; i++) {
                var coroutine = _enumerators[i];
                coroutine?.Reset();
            }
        }

        /// <summary>
        /// コルーチン進行
        /// </summary>
        /// <returns>次の処理があるか？</returns>
        bool IEnumerator.MoveNext() {
            var finished = true;

            for (var i = 0; i < _enumerators.Length; i++) {
                var enumerator = _enumerators[i];

                if (enumerator == null) {
                    continue;
                }

                if (!enumerator.MoveNext()) {
                    _enumerators[i] = null;
                }
                else {
                    finished = false;
                }
            }

            IsDone = finished;
            return !IsDone;
        }
    }
}