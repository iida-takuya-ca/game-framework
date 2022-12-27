using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// コンストレイントの基底(Runtime追加用)
    /// </summary>
    [ExecuteAlways]
    public abstract class RuntimeConstraintExpression : IExpression {
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
        protected RuntimeConstraintExpression(Transform owner, string targetName = "") {
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
        protected RuntimeConstraintExpression(Transform owner, Transform target)
            : this(owner, target != null ? target.name : "") {
        }
        
        /// <summary>
        /// ジョブ用ConstraintのTargetハンドルを生成
        /// </summary>
        protected ConstraintTargetHandle CreateTargetHandle(Animator animator) {
            NormalizeWeights();

            var infos = _targetInfos.Where(x => x.target != null)
                .ToArray();

            // Job用ターゲットハンドル構築
            var handle = new ConstraintTargetHandle();
            handle.CreateTargetInfos(infos.Length);

            for (var i = 0; i < infos.Length; i++) {
                handle.SetTargetInfo(i, new ConstraintTargetHandle.TargetInfo {
                    normalizedWeight = infos[i].normalizedWeight,
                    targetHandle = animator.BindStreamTransform(infos[i].target)
                });
            }

            return handle;
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