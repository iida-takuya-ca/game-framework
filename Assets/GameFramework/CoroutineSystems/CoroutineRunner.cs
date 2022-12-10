using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// コルーチン実行クラス
    /// </summary>
    public class CoroutineRunner : IDisposable {
        // コルーチン情報
        private class CoroutineInfo {
            public Coroutine coroutine;
            public CancellationToken cancellationToken;
            public Action<Exception> onError;
            public Action onCanceled;
            public Action onCompleted;

            public bool isCanceled;
            public Exception exception;
            public bool isCompleted;

            public bool IsCanceled =>
                isCanceled || (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested);
            public bool IsDone => IsCanceled || exception != null || isCompleted;
        }

        // 制御中のコルーチン
        private readonly List<CoroutineInfo> _coroutineInfos = new List<CoroutineInfo>();
        // 更新が完了したコルーチンのIDを保持するリスト
        private readonly List<int> _cachedRemoveIndices = new List<int>();

        /// <summary>
        /// コルーチンの開始
        /// </summary>
        /// <param name="enumerator">実行する非同期処理</param>
        /// <param name="onCompleted">完了時の通知</param>
        /// <param name="onCanceled">キャンセル時の通知</param>
        /// <param name="onError">エラー時の通知</param>
        public Coroutine StartCoroutine(IEnumerator enumerator, Action onCompleted = null,
            Action onCanceled = null, Action<Exception> onError = null, CancellationToken cancellationToken = default) {
            if (enumerator == null) {
                Debug.LogError("Invalid coroutine func.");
                return null;
            }

            // コルーチンの追加
            var coroutineInfo = new CoroutineInfo {
                coroutine = new Coroutine(enumerator),
                cancellationToken = cancellationToken,
                onCompleted = onCompleted,
                onCanceled = onCanceled
            };

            // エラーハンドリング用のアクションが無い時はログを出力するようにする
            coroutineInfo.onError = onError ?? Debug.LogException;

            _coroutineInfos.Add(coroutineInfo);

            return coroutineInfo.coroutine;
        }

        /// <summary>
        /// コルーチンの強制停止
        /// </summary>
        /// <param name="coroutine">停止させる対象のCoroutine</param>
        public void StopCoroutine(Coroutine coroutine) {
            if (coroutine == null) {
                Debug.LogError("Invalid coroutine instance.");
                return;
            }

            var foundIndex = _coroutineInfos.FindIndex(x => x.coroutine == coroutine);
            if (foundIndex < 0) {
                Debug.LogError("Not found coroutine.");
                return;
            }

            var info = _coroutineInfos[foundIndex];
            if (info.IsDone) {
                return;
            }

            // キャンセル処理
            CancelCoroutine(info);
        }

        /// <summary>
        /// コルーチンの全停止
        /// </summary>
        public void StopAllCoroutines() {
            // 降順にキャンセルしていく
            for (var i = _coroutineInfos.Count - 1; i >= 0; i--) {
                var info = _coroutineInfos[i];
                if (info.IsDone) {
                    continue;
                }

                // キャンセル処理
                CancelCoroutine(info);
            }

            _coroutineInfos.Clear();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 全コルーチン停止
            StopAllCoroutines();
        }

        /// <summary>
        /// コルーチン更新処理
        /// </summary>
        public void Update() {
            // コルーチンの更新
            _cachedRemoveIndices.Clear();

            // 昇順で更新
            for (var i = 0; i < _coroutineInfos.Count; i++) {
                var coroutineInfo = _coroutineInfos[i];
                var coroutine = coroutineInfo.coroutine;

                if (coroutineInfo.IsCanceled) {
                    _cachedRemoveIndices.Add(i);
                    continue;
                }

                try {
                    if (!((IEnumerator)coroutine).MoveNext()) {
                        coroutineInfo.isCompleted = true;
                        // 完了通知
                        coroutineInfo.onCompleted?.Invoke();
                        _cachedRemoveIndices.Add(i);
                    }
                }
                catch (Exception exception) {
                    coroutineInfo.exception = exception;
                    // エラー終了通知
                    coroutineInfo.onError?.Invoke(exception);
                    _cachedRemoveIndices.Add(i);
                }
            }

            // 降順でインスタンスクリア
            for (var i = _cachedRemoveIndices.Count - 1; i >= 0; i--) {
                _coroutineInfos.RemoveAt(_cachedRemoveIndices[i]);
            }
        }

        /// <summary>
        /// コルーチンキャンセル処理
        /// </summary>
        private void CancelCoroutine(CoroutineInfo coroutineInfo) {
            coroutineInfo.isCanceled = true;
            coroutineInfo.onCanceled?.Invoke();
        }
    }
}