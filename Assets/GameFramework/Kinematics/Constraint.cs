using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// コンストレイントの基底
    /// </summary>
    [ExecuteAlways]
    public abstract class Constraint : MonoBehaviour, IConstraint {
        // 更新モード
        public enum Mode {
            Update,
            LateUpdate,
            Manual,
        }

        // ターゲット情報
        [Serializable]
        public class TargetSource {
            [Tooltip("追従先")] public Transform target;
            [Tooltip("追従先検索用相対パス")] public string targetPath;
            [Tooltip("影響率")] public float weight = 1.0f;

            // 正規化済みWeight
            public float NormalizedWeight { get; set; } = 1.0f;
        }

        [SerializeField, Tooltip("更新モード")] private Mode _updateMode = Mode.LateUpdate;
        [SerializeField, Tooltip("有効化")] private bool _active = false;
        [SerializeField, Tooltip("ターゲットリスト")] private TargetSource[] _sources = Array.Empty<TargetSource>();

        // Weightが正規化済みか
        private bool _normalized = false;

        // 更新モード
        public Mode UpdateMode {
            get => _updateMode;
            set => _updateMode = value;
        }

        // ターゲットリスト
        public TargetSource[] Sources {
            set {
                _sources = value;
                _normalized = false;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void ManualUpdate() {
            if (_updateMode != Mode.Manual) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// ターゲットのTransform参照をリフレッシュ
        /// ※Transformの組み替えなどを行った場合に使用(相対Path解決)
        /// </summary>
        public void RefreshTargets(Transform root) {
            foreach (var source in _sources) {
                if (source.target != null) {
                    continue;
                }

                source.target = transform.Find(source.targetPath);
            }
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public abstract void TransferOffset();

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public abstract void ResetOffset();

        /// <summary>
        /// Transformを反映
        /// </summary>
        public abstract void ApplyTransform();

        /// <summary>
        /// ターゲット座標
        /// </summary>
        protected Vector3 GetTargetPosition() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
                _normalized = true;
            }

            var position = Vector3.zero;

            foreach (var source in _sources) {
                if (source.target == null) {
                    continue;
                }

                position += source.target.position * source.NormalizedWeight;
            }

            return position;
        }

        /// <summary>
        /// ターゲット姿勢
        /// </summary>
        protected Quaternion GetTargetRotation() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
                _normalized = true;
            }

            var rotation = Quaternion.identity;

            foreach (var source in _sources) {
                if (source.target == null) {
                    continue;
                }

                rotation *= Quaternion.Slerp(Quaternion.identity, source.target.rotation,
                    source.NormalizedWeight);
            }

            return rotation;
        }

        /// <summary>
        /// ターゲットローカルスケール
        /// </summary>
        protected Vector3 GetTargetLocalScale() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
                _normalized = true;
            }

            var scale = Vector3.zero;

            foreach (var source in _sources) {
                if (source.target == null) {
                    continue;
                }

                scale += source.target.localScale * source.NormalizedWeight;
            }

            return scale;
        }

        /// <summary>
        /// ターゲットのWeightを合計で1になるように正規化
        /// </summary>
        private void NormalizeWeights() {
            var totalWeight = _sources.Where(x => x.target != null).Sum(x => x.weight);

            if (totalWeight <= float.Epsilon) {
                return;
            }

            foreach (var source in _sources) {
                source.NormalizedWeight = source.weight / totalWeight;
            }
        }

        /// <summary>
        /// 更新処理(内部用)
        /// </summary>
        private void UpdateInternal() {
            if (_sources.Length <= 0) {
                return;
            }

#if UNITY_EDITOR
            if (!_active && !Application.isPlaying) {
                return;
            }
#endif

            ApplyTransform();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (_updateMode != Mode.Update && _updateMode != Mode.Manual && Application.isPlaying) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            if (_updateMode != Mode.LateUpdate) {
                return;
            }

            UpdateInternal();
        }
    }
}