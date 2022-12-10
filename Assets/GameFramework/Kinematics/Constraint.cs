using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

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
            [Tooltip("追従先")]
            public Transform target;
            [Tooltip("追従先検索用相対パス")]
            public string targetPath;
            [Tooltip("影響率")]
            public float weight = 1.0f;
        }

        // ターゲット情報
        private class TargetInfo {
            // ターゲットの元情報
            public TargetSource source = new TargetSource();
            // 参照するTarget
            public Transform target;
            // 正規化済みのWeight
            public float normalizedWeight;
        }

        [SerializeField, Tooltip("更新モード")]
        private Mode _updateMode = Mode.LateUpdate;
        [SerializeField, Tooltip("有効化")]
        private bool _active = false;
        [SerializeField, Tooltip("ターゲットリスト")]
        private TargetSource[] _sources = Array.Empty<TargetSource>();

        // ターゲット情報リスト
        private TargetInfo[] _targetInfos = Array.Empty<TargetInfo>();
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
                _targetInfos = value.Select(x => new TargetInfo {
                        source = x,
                    })
                    .ToArray();
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

            _active = false;

            for (var i = 0; i < _targetInfos.Length; i++) {
                var info = _targetInfos[i];
                if (info.target != null) {
                    _active = true;
                    continue;
                }

                info.target = _sources[i].target;

                if (info.target != null) {
                    _active = true;
                }
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
        /// Constraintの共通ジョブパラメータ取得
        /// </summary>
        protected ConstraintAnimationJobParameter CreateJobParameter(Animator animator) {
            var infos = _targetInfos.Where(x => x.target != null)
                .ToArray();

            // Job用パラメータ再構築
            var parameter = new ConstraintAnimationJobParameter();
            parameter.targetInfos =
                new NativeArray<ConstraintAnimationJobParameter.TargetInfo>(infos.Length, Allocator.Persistent);

            for (var i = 0; i < infos.Length; i++) {
                parameter.targetInfos[i] = new ConstraintAnimationJobParameter.TargetInfo {
                    normalizedWeight = infos[i].normalizedWeight,
                    targetHandle = animator.BindSceneTransform(infos[i].target)
                };
            }

            return parameter;
        }

        /// <summary>
        /// ターゲット座標
        /// </summary>
        protected Vector3 GetTargetPosition() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
                _normalized = true;
            }

            var position = Vector3.zero;

            foreach (var info in _targetInfos) {
                if (info.target == null) {
                    continue;
                }

                position += info.target.position * info.normalizedWeight;
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

            foreach (var info in _targetInfos) {
                if (info.target == null) {
                    continue;
                }

                rotation *= Quaternion.Slerp(Quaternion.identity, info.target.rotation,
                    info.normalizedWeight);
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

            foreach (var info in _targetInfos) {
                if (info.target == null) {
                    continue;
                }

                scale += info.target.localScale * info.normalizedWeight;
            }

            return scale;
        }

        /// <summary>
        /// ターゲットのWeightを合計で1になるように正規化
        /// </summary>
        private void NormalizeWeights() {
            var totalWeight = _targetInfos.Where(x => x.target != null).Sum(x => x.source.weight);

            if (totalWeight <= float.Epsilon) {
                return;
            }

            foreach (var info in _targetInfos) {
                info.normalizedWeight = info.source.weight / totalWeight;
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