using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Bodyの骨制御クラス
    /// </summary>
    public class BoneController : SerializedBodyController {
        [SerializeField, Tooltip("ルート骨")]
        private Transform _rootTransform = default;
        public Transform Root => _rootTransform;

        // 実行優先度
        public override int ExecutionOrder => 15;
    }
}