using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体用制御用インターフェース
    /// </summary>
    public interface IProjectile {
        // 座標
        Vector3 Position { get; }
        // 姿勢
        Quaternion Rotation { get; }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void Start();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool Update(float deltaTime);

        /// <summary>
        /// 飛翔終了
        /// </summary>
        void Stop();
    }
}