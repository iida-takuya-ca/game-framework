using System.Collections.Generic;
using UnityEngine;
using GameFramework.BodySystems;
using GameFramework.Kinematics;

namespace GameFramework.BodySystems {
  /// <summary>
  /// Bodyの骨制御クラス
  /// </summary>
  public class BoneController : SerializedBodyController {
    private class ConstraintInfo {
      public List<IConstraint> constraints = new List<IConstraint>();
    }

    [SerializeField, Tooltip("ルート骨")]
    private Transform _rootTransform = default;
    public Transform Root => _rootTransform;

    // Constraintしている骨の情報
    private Dictionary<MeshParts, ConstraintInfo> _constraintInfos =
      new Dictionary<MeshParts, ConstraintInfo>();

    // 実行優先度
    public override int ExecutionOrder => 1;

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void InitializeInternal() {
      // MeshPartsの追加/削除時にUniqueBoneにConstraintをつける
      var meshController = Body.GetController<MeshController>();
      var constraintController = Body.GetController<ConstraintController>();

      meshController.OnAddedMeshParts += meshParts => {
        var constraintInfo = default(ConstraintInfo);

        foreach (var info in meshParts.uniqueBoneInfos) {
          if (info.constraintMask == 0 || info.targetBones.Length <= 0) {
            continue;
          }

          if (constraintInfo == null) {
            constraintInfo = new ConstraintInfo();
            _constraintInfos[meshParts] = constraintInfo;
          }

          foreach (var bone in info.targetBones) {
            var targetBoneName = meshController.GetOriginalBoneName(bone);
            var constraint = new ParentRuntimeConstraint(bone, targetBoneName);
            constraint.DisablePosition =
              (info.constraintMask & MeshParts.ConstraintMasks.Position) == 0;
            constraint.DisableRotation =
              (info.constraintMask & MeshParts.ConstraintMasks.Rotation) == 0;
            constraint.DisableLocalScale =
              (info.constraintMask & MeshParts.ConstraintMasks.LocalScale) == 0;
            constraintInfo.constraints.Add(constraint);
            constraintController.RegisterConstraint(constraint);
          }
        }
      };
      meshController.OnRemovedMeshParts += meshParts => {
        if (_constraintInfos.TryGetValue(meshParts, out var info)) {
          foreach (var constraint in info.constraints) {
            constraintController.UnregisterConstraint(constraint);
          }

          _constraintInfos.Remove(meshParts);
        }
      };
    }
  }
}
