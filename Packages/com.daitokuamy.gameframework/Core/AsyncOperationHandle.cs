using System;
using System.Collections;

namespace GameFramework.Core {
    /// <summary>
    /// 非同期処理汎用オペレーター
    /// </summary>
    /// 
    /// <example>
    /// public class OperatorClass {
    ///     public AsyncOperationHandle HogeAsync() {
    ///         var op = new AsyncOperator();
    ///         // 非同期処理
    ///         InternalAsync(() => {
    ///             // 非同期完了
    ///             op.Completed();
    ///         }
    ///         return op;
    ///     }
    /// }
    ///
    /// public class SampleClass {
    ///     public IEnumerator SetupRoutine() {
    ///         var operatorClass = new OperatorClass();
    ///         var handle = operatorClass.HogeAsync();
    ///         yield return handle;
    ///     }
    /// }
    /// </example>
    public class AsyncOperator {
        // 正常終了か
        public bool IsCompleted { get; private set; }
        // エラー終了か
        public Exception Exception { get; private set; }
        // 完了しているか
        public bool IsDone => IsCompleted || Exception != null;

        // 完了通知イベント
        public event Action OnCompletedEvent;
        // キャンセル通知イベント
        public event Action<Exception> OnCanceledEvent;

        /// <summary>
        /// ハンドルへの暗黙型変換
        /// </summary>
        public static implicit operator AsyncOperationHandle(AsyncOperator source) {
            return source.GetHandle();
        }

        /// <summary>
        /// ハンドルの取得
        /// </summary>
        public AsyncOperationHandle GetHandle() {
            return new AsyncOperationHandle(this);
        }

        /// <summary>
        /// 完了時に呼び出す処理
        /// </summary>
        public void Completed() {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate completion action.");
            }

            IsCompleted = true;
            OnCompletedEvent?.Invoke();
            OnCompletedEvent = null;
            OnCanceledEvent = null;
        }

        /// <summary>
        /// キャンセル時に呼び出す処理
        /// </summary>
        /// <param name="exception">キャンセル原因</param>
        public void Canceled(Exception exception = null) {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate cancel action.");
            }

            if (exception == null) {
                exception = new OperationCanceledException("Canceled operation");
            }

            Exception = exception;
            OnCanceledEvent?.Invoke(exception);
            OnCompletedEvent = null;
            OnCanceledEvent = null;
        }
    }

    /// <summary>
    /// 非同期処理ハンドル用オペレーター
    /// </summary>
    public class AsyncOperator<T> {
        // 結果
        public T Result { get; private set; }
        // 正常終了か
        public bool IsCompleted { get; private set; }
        // エラー終了か
        public Exception Exception { get; private set; }
        // 完了しているか
        public bool IsDone => IsCompleted || Exception != null;

        // 完了通知イベント
        public event Action<T> OnCompletedEvent;
        // キャンセル通知イベント
        public event Action<Exception> OnCanceledEvent;

        /// <summary>
        /// ハンドルへの暗黙型変換
        /// </summary>
        public static implicit operator AsyncOperationHandle<T>(AsyncOperator<T> source) {
            return source.GetHandle();
        }

        /// <summary>
        /// ハンドルの取得
        /// </summary>
        public AsyncOperationHandle<T> GetHandle() {
            return new AsyncOperationHandle<T>(this);
        }

        /// <summary>
        /// 完了時に呼び出す処理
        /// </summary>
        public void Completed(T result) {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate completion action.");
            }

            Result = result;
            IsCompleted = true;
            OnCompletedEvent?.Invoke(result);
            OnCompletedEvent = null;
            OnCanceledEvent = null;
        }

        /// <summary>
        /// キャンセル時に呼び出す処理
        /// </summary>
        /// <param name="exception">キャンセル原因</param>
        public void Canceled(Exception exception = null) {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate cancel action.");
            }

            if (exception == null) {
                exception = new OperationCanceledException("Canceled operation");
            }

            Exception = exception;
            OnCanceledEvent?.Invoke(exception);
            OnCompletedEvent = null;
            OnCanceledEvent = null;
        }
    }

    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public readonly struct AsyncOperationHandle : IProcess {
        private readonly AsyncOperator _asyncOperator;

        // 完了しているか
        public bool IsDone => _asyncOperator == null || _asyncOperator.IsDone;
        // キャンセル時のエラー
        public Exception Exception => _asyncOperator?.Exception;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>
        /// 通知の購読
        /// </summary>
        /// <param name="onCompleted">完了時通知</param>
        /// <param name="onCanceled">キャンセル時通知</param>
        public void ListenTo(Action onCompleted, Action<Exception> onCanceled = null) {
            if (_asyncOperator == null || _asyncOperator.IsDone) {
                return;
            }

            if (onCompleted != null) {
                _asyncOperator.OnCompletedEvent += onCompleted;
            }

            if (onCanceled != null) {
                _asyncOperator.OnCanceledEvent += onCanceled;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asyncOperator">非同期通知用インスタンス</param>
        internal AsyncOperationHandle(AsyncOperator asyncOperator) {
            _asyncOperator = asyncOperator;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public readonly struct AsyncOperationHandle<T> : IProcess<T> {
        private readonly AsyncOperator<T> _asyncOperator;

        // 結果
        public T Result => _asyncOperator != null ? _asyncOperator.Result : default;
        // 完了しているか
        public bool IsDone => _asyncOperator == null || _asyncOperator.IsDone;
        // キャンセル時のエラー
        public Exception Exception => _asyncOperator?.Exception;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>
        /// 通知の購読
        /// </summary>
        /// <param name="onCompleted">完了時通知</param>
        /// <param name="onCanceled">キャンセル時通知</param>
        public void ListenTo(Action<T> onCompleted, Action<Exception> onCanceled = null) {
            if (_asyncOperator == null || _asyncOperator.IsDone) {
                return;
            }

            if (onCompleted != null) {
                _asyncOperator.OnCompletedEvent += onCompleted;
            }

            if (onCanceled != null) {
                _asyncOperator.OnCanceledEvent += onCanceled;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asyncOperator">非同期通知用インスタンス</param>
        internal AsyncOperationHandle(AsyncOperator<T> asyncOperator) {
            _asyncOperator = asyncOperator;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }
}