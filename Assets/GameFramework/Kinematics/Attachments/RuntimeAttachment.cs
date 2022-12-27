using System;
using System.Linq;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Attachmentの基底(Runtime追加用)
    /// </summary>
    [ExecuteAlways]
    public abstract class RuntimeAttachment : IAttachment {
        // ターゲット元情報
        public class TargetSource {
            // 追従先の名前
            public string targetName;
            // 反映率
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

        // ターゲット情報リスト
        private TargetInfo[] _targetInfos = Array.Empty<TargetInfo>();
        // Weightが正規化済みか
        private bool _normalized = false;
        // 有効か
        private bool _active;

        // 制御対象
        public Transform Owner { get; set; }
        // ターゲット元リスト
        public TargetSource[] Sources {
            set {
                _targetInfos = value.Select(x => new TargetInfo {
                        source = x,
                    })
                    .ToArray();
                _normalized = false;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象</param>
        /// <param name="targetName">追従対象の名前</param>
        protected RuntimeAttachment(Transform owner, string targetName = "") {
            Owner = owner;

            if (!string.IsNullOrEmpty(targetName)) {
                Sources = new[] {
                    new TargetSource {
                        targetName = targetName,
                        weight = 1.0f
                    }
                };
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象</param>
        /// <param name="target">追従対象</param>
        protected RuntimeAttachment(Transform owner, Transform target)
            : this(owner, target != null ? target.name : "") {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void ManualUpdate() {
            if (Owner == null || !_active) {
                return;
            }

            ApplyTransform();
        }

        /// <summary>
        /// ターゲットのTransform参照をリフレッシュ
        /// ※Transformの組み替えなどを行った場合に使用(相対Path解決)
        /// </summary>
        public void RefreshTargets(Transform root) {
            // 再起的にターゲットを探す
            Transform FindTarget(Transform parent, string name) {
                var result = parent.Find(name);

                if (result != null) {
                    return result;
                }

                for (var i = 0; i < parent.childCount; i++) {
                    result = FindTarget(parent.GetChild(i), name);

                    if (result != null) {
                        return result;
                    }
                }

                return null;
            }

            _active = false;

            foreach (var info in _targetInfos) {
                if (info.target != null) {
                    _active = true;
                    continue;
                }

                info.target = FindTarget(root, info.source.targetName);

                if (info.target != null) {
                    _active = true;
                }
            }
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        protected abstract void ApplyTransform();

        /// <summary>
        /// ターゲット座標
        /// </summary>
        protected Vector3 GetTargetPosition() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
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

            _normalized = true;
        }
    }
}