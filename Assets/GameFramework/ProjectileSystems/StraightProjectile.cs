using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 直進用Projectile
    /// </summary>
    public class StraightProjectile : IProjectile {
        private readonly Vector3 _startPoint;
        private readonly Vector3 _startVelocity;
        private readonly Vector3 _acceleration;
        private readonly float _maxDistance;

        private Vector3 _velocity;
        private float _distance;

        // 座標
        public Vector3 Position { get; private set; }
        // 姿勢
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="startVelocity">初速度</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="maxDistance">最大飛翔距離</param>
        public StraightProjectile(Vector3 startPoint, Vector3 startVelocity, Vector3 acceleration, float maxDistance) {
            _startPoint = startPoint;
            _startVelocity = startVelocity;
            _acceleration = acceleration;
            _maxDistance = maxDistance;
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            Position = _startPoint;
            _velocity = _startVelocity;
            _distance = 0.0f;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool IProjectile.Update(float deltaTime) {
            // 速度更新
            _velocity += _acceleration * deltaTime;
            
            // 移動量
            var deltaPos = _velocity * deltaTime;
            var restDistance = _maxDistance - _distance;
            if (deltaPos.sqrMagnitude > restDistance * restDistance) {
                deltaPos *= restDistance / deltaPos.magnitude;
            }

            // 座標更新
            Position += deltaPos;
            
            // 向き更新
            if (deltaPos.sqrMagnitude > float.Epsilon) {
                Rotation = Quaternion.LookRotation(deltaPos);
            }

            // 距離更新
            _distance += _velocity.magnitude * deltaTime;

            return _distance < _maxDistance;
        }

        /// <summary>
        /// 飛翔終了
        /// </summary>
        void IProjectile.Stop() {
        }
    }
}