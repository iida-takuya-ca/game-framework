using System.Collections.Generic;
using GameFramework.Kinematics;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Bodyの骨制御クラス
    /// </summary>
    public class BoneController : SerializedBodyController {
        [SerializeField, Tooltip("ルート骨")]
        private Transform _rootTransform = default;

        // 追従用のAttachment(todo:AnimationJobにする)
        private Dictionary<MeshParts, List<ParentRuntimeAttachment>> _attachments = new();

        public Transform Root => _rootTransform;

        // 実行優先度
        public override int ExecutionOrder => 15;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            var meshController = Body.GetController<MeshController>();
            meshController.OnAddedMeshParts += parts => {
                if (!_attachments.TryGetValue(parts, out var list)) {
                    list = new List<ParentRuntimeAttachment>();
                    _attachments[parts] = list;
                }
                list.Clear();
                
                foreach (var info in parts.uniqueBoneInfos) {
                    foreach (var bone in info.targetBones) {
                        if (bone == null) {
                            continue;
                        }

                        var attachment = new ParentRuntimeAttachment(bone);
                        attachment.Settings.mask = (TransformMasks)info.constraintMask;
                        list.Add(attachment);
                    }
                }
            };
            
            meshController.OnRemovedMeshParts += parts => {
                _attachments.Remove(parts);
            };
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            foreach (var list in _attachments.Values) {
                foreach (var attachment in list) {
                    attachment.ManualUpdate();
                }
            }
        }
    }
}