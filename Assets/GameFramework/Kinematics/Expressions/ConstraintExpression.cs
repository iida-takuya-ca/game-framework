using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.Kinematics {
    /// <summary>
    /// コンストレイントの基底
    /// </summary>
    [ExecuteAlways]
    public abstract class ConstraintExpression : MonoBehaviour, IExpression {
        // ターゲット情報
        [Serializable]
        public class TargetSource {
            [Tooltip("追従先")]
            public Transform target;
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

        [SerializeField, Tooltip("ターゲットリスト")]
        private TargetSource[] _sources = Array.Empty<TargetSource>();

        // ターゲット情報リスト
        private TargetInfo[] _targetInfos = Array.Empty<TargetInfo>();
        // Weightが正規化済みか
        private bool _normalized = false;
        
        // ターゲットリスト
        public TargetSource[] Sources {
            set {
                _sources = value;
                _targetInfos = value.Select(x => new TargetInfo {
                        source = x,
                        target = x.target
                    })
                    .ToArray();
                _normalized = false;
            }
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

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            Sources = _sources;
        }

        /// <summary>
        /// 値変更時の処理
        /// </summary>
        private void OnValidate() {
            Sources = _sources;
        }
    }
}