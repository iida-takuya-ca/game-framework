using System;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// Entity拡張用コンポーネントインターフェース
    /// </summary>
    public interface IEntityComponent : IDisposable {
        /// <summary>
        /// Entityに接続された時の処理
        /// </summary>
        /// <param name="entity">接続対象のEntity</param>
        void Attached(Entity entity);

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        void Activate();

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Entityから接続外れた時の処理
        /// </summary>
        /// <param name="entity">接続外れたEntity</param>
        void Detached(Entity entity);
    }
}