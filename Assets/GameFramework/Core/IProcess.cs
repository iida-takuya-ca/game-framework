using System;

namespace GameFramework.Core {
    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public interface IProcess {
        // 完了しているか
        bool IsDone { get; }
        // エラー内容
        Exception Exception { get; }
    }
}