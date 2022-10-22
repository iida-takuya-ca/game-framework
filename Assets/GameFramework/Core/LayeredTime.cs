using System;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// 時間の階層管理用クラス
    /// </summary>
    public class LayeredTime : IDisposable {
        // 階層用の親TimeLayer
        private LayeredTime _parent;
        // 自身の持つTimeLayer
        private float _localTimeScale = 1.0f;

        // TimeScaleの変更通知
        public event Action<float> OnChangedTimeScale;
        // 内部用TimeScaleの変更通知（通知タイミングを揃えるため）
        private event Action<float> OnChangedTimeScaleInternal;
        
        // 自身のTimeScale
        public float LocalTimeScale {
            get => _localTimeScale;
            set {
                _localTimeScale = Mathf.Max(0.0f, value);
                var timeScale = TimeScale;
                OnChangedTimeScaleInternal?.Invoke(timeScale);
                OnChangedTimeScale?.Invoke(timeScale);
            }
        }
        // 親階層を考慮したTimeScale
        public float TimeScale => (_parent?.TimeScale ?? 1.0f) * _localTimeScale;
        // 現フレームのDeltaTime
        public float DeltaTime => Time.deltaTime * TimeScale;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親となるTimeLayer, 未指定の場合UnityEngine.Timeに直接依存</param>
        public LayeredTime(LayeredTime parent = null) {
            SetParent(parent);
        }

        /// <summary>
        /// 親のTimeLayerの設定
        /// </summary>
        /// <param name="parent">親となるTimeLayer, 未指定の場合UnityEngine.Timeに直接依存</param>
        public void SetParent(LayeredTime parent) {
            if (_parent != null) {
                _parent.OnChangedTimeScaleInternal -= OnChangedTimeScaleInternal;
                _parent = null;
            }
            
            _parent = parent;

            if (_parent != null) {
                _parent.OnChangedTimeScaleInternal += OnChangedTimeScaleInternal;
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_parent != null) {
                _parent.OnChangedTimeScaleInternal -= OnChangedTimeScaleInternal;
                _parent = null;
            }
        }
    }
}
