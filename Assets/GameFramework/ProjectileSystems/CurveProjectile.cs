using System;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// カーブ制御Projectile
    /// </summary>
    public class CurveProjectile : IProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("開始座標")]
            public Vector3 startPoint;
            [Tooltip("終了座標")]
            public Vector3 endPoint;
            [Tooltip("振動カーブ(-1〜1)")]
            public AnimationCurve vibrationCurve;
            [Tooltip("振動幅")]
            public float amplitude;
            [Tooltip("奥行きカーブ(Last=1)")]
            public AnimationCurve depthCurve;
            [Tooltip("ねじりカーブ(-1〜1)")]
            public AnimationCurve rollCurve;
            [Tooltip("到達時間")]
            public float duration;
        }

        private readonly Vector3 _startPoint;
        private Vector3 _endPoint;
        private readonly AnimationCurve _vibrationCurve;
        private readonly float _amplitude;
        private readonly AnimationCurve _depthCurve;
        private readonly AnimationCurve _rollCurve;
        private readonly float _duration;

        private float _timer;
        private Vector3 _prevPosition;

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
        /// <param name="vibrationCurve">振動カーブ(-1～1)</param>
        /// <param name="amplitude">振幅</param>
        /// <param name="depthCurve">奥行きカーブ(1でTargetPoint)</param>
        /// <param name="rollCurve">ねじれカーブ(-1～1)</param>
        /// <param name="duration"></param>
        public CurveProjectile(Vector3 startPoint, Vector3 endPoint, AnimationCurve vibrationCurve, float amplitude,
            AnimationCurve depthCurve, AnimationCurve rollCurve, float duration) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _vibrationCurve = vibrationCurve;
            _amplitude = amplitude;
            _depthCurve = depthCurve;
            _rollCurve = rollCurve;
            _duration = duration;

            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_endPoint - _startPoint);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">初期化パラメータ</param>
        public CurveProjectile(Context context)
            : this(context.startPoint, context.endPoint, context.vibrationCurve, context.amplitude, context.depthCurve,
                context.rollCurve, context.duration) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_endPoint - _startPoint);
            _timer = _duration;
            _prevPosition = _startPoint;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool IProjectile.Update(float deltaTime) {
            var vector = _endPoint - _startPoint;
            var distance = vector.magnitude;

            if (distance <= float.Epsilon) {
                Position = _endPoint;
                Rotation = Quaternion.identity;
                return false;
            }

            // 時間更新
            _timer += deltaTime;

            var ratio = Mathf.Clamp01(1 - _timer / _duration);

            // 位置計算
            var vibration = _vibrationCurve.Evaluate(ratio) * _amplitude;
            var depth = _depthCurve.Evaluate(ratio);
            var roll = _rollCurve.Evaluate(ratio);
            var forward = vector.normalized;
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var up = Vector3.Cross(forward, right);
            var relativePos = Vector3.zero;
            var radian = roll * Mathf.PI;
            relativePos += forward * depth;
            relativePos += up * (Mathf.Cos(radian) * vibration);
            relativePos += right * (Mathf.Sin(radian) * vibration);
            Position = _startPoint + relativePos;
            Rotation = Quaternion.LookRotation(Position - _prevPosition, up);
            _prevPosition = Position;

            return _timer < _duration;
        }

        /// <summary>
        /// 飛翔終了
        /// </summary>
        void IProjectile.Stop() {
        }
    }
}