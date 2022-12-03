using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 移動制御用コントローラ
    /// </summary>
    public class MoveController : IDisposable {
        private Transform _owner;
        private Transform _target;
        private Vector3? _targetPosition;
        private float _reachDistance;
        
        // 速度
        public float Velocity { get; set; }
        // 角速度(度)
        public float AngularVelocity { get; set; }
        // 移動中
        public bool IsMoving { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveController(Transform owner, float velocity, float angularVelocity, float reachDistance) {
            _owner = owner;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _owner = null;
            _target = null;
            _targetPosition = null;
            IsMoving = false;
        }

        /// <summary>
        /// ターゲット座標の設定
        /// </summary>
        public void SetTarget(Vector3 target) {
            _target = null;
            _targetPosition = target;
            IsMoving = true;
        }

        /// <summary>
        /// ターゲットの設定
        /// </summary>
        public void SetTarget(Transform target, Vector3 offset) {
            _target = target;
            _targetPosition = offset;
            IsMoving = true;
        }

        /// <summary>
        /// ターゲットのクリア
        /// </summary>
        public void ClearTarget() {
            _target = null;
            _targetPosition = null;
            IsMoving = false;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            if (!_targetPosition.HasValue) {
                return;
            }
            
            var target = _targetPosition.Value;
            if (_target != null) {
                target = _target.TransformPoint(target);
            }
            
            var vector = target - _owner.position;
            IsMoving = vector.sqrMagnitude > (_reachDistance * _reachDistance);

            // 既に到着している
            if (!IsMoving) {
                return;
            }
            
            // 向きを揃える
            var angles = _owner.eulerAngles;
            var targetAngle = Mathf.Atan2(vector.x, vector.z) * Mathf.Rad2Deg;
            angles.y = Mathf.MoveTowardsAngle(angles.y, targetAngle, AngularVelocity * deltaTime);
            _owner.eulerAngles = angles;
            
            // 距離を詰める
            var forward = _owner.forward;
            var maxDistance = Vector3.Dot(forward, vector);
            if (maxDistance <= 0.0f) {
                // 行き過ぎている
                return;
            }
            var distance = Mathf.Min(maxDistance * 0.7f, Velocity * deltaTime);
            _owner.position += forward * distance;
        }
    }
}
