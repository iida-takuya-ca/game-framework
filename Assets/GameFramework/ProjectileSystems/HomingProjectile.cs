using System;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ホーミング制御Projectile
    /// </summary>
    public class HomingProjectile : IProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("開始座標")]
            public Vector3 startPoint;
            [Tooltip("終了座標")]
            public Vector3 endPoint;
            [Tooltip("初速度")]
            public Vector3 startVelocity;
            [Tooltip("推進力")]
            public float propulsion;
            [Tooltip("抵抗力")]
            public float damping;
            [Tooltip("加速度の最大値")]
            public float maxAcceleration;
            [Tooltip("最大飛翔距離")]
            public float maxDistance;
        }

        private readonly Vector3 _startPoint;
        private readonly Vector3 _startVelocity;
        private readonly float _propulsion;
        private readonly float _damping;
        private readonly float _maxAcceleration;
        private readonly float _maxDistance;

        private Vector3 _endPoint;
        private Vector3 _velocity;
        private float _distance;

        // 座標
        public Vector3 Position { get; private set; }
        // 姿勢
        public Quaternion Rotation { get; private set; }

        // 終端座標
        public Vector3 EndPoint {
            get => _endPoint;
            set => _endPoint = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">開始座標</param>
        /// <param name="endPoint">終了座標</param>
        /// <param name="startVelocity">初速度</param>
        /// <param name="propulsion">推進力</param>
        /// <param name="damping">ダンピング力</param>
        /// <param name="maxAcceleration">最大加速度</param>
        /// <param name="maxDistance">最大距離</param>
        public HomingProjectile(Vector3 startPoint, Vector3 endPoint, Vector3 startVelocity, float propulsion, float damping, float maxAcceleration, float maxDistance) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _startVelocity = startVelocity;
            _propulsion = propulsion;
            _damping = damping;
            _maxAcceleration = maxAcceleration;
            _maxDistance = maxDistance;

            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_startVelocity);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">初期化用パラメータ</param>
        public HomingProjectile(Context context)
            : this(context.startPoint, context.endPoint, context.startVelocity, context.propulsion, context.damping,
                context.maxAcceleration, context.maxDistance)
        {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_endPoint - _startPoint);
            _velocity = _startVelocity;
            _distance = 0.0f;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool IProjectile.Update(float deltaTime) {
            // 進行方向を補間するための加速度を求める
            var vector = _endPoint - Position;
            var forward = _velocity.normalized;
            var acceleration = vector - forward * Vector3.Dot(vector, forward);
            var accelMagnitude = acceleration.magnitude;
            if (accelMagnitude > 1.0f) {
                acceleration *= _maxAcceleration / accelMagnitude;
            }
            else {
                acceleration *= _maxAcceleration;
            }
            
            // 加速度を補間
            acceleration += forward * _propulsion - _velocity * _damping;
            
            // 速度に反映
            _velocity += acceleration * deltaTime;
            
            // 座標更新
            var deltaPos = _velocity * deltaTime; 
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