using UnityEngine;

namespace GameFramework.Kinematics {
  /// <summary>
  /// 追従コンストレイント
  /// </summary>
  public class ParentRuntimeConstraint : RuntimeConstraint {
    // コンストレイント設定
    public class ConstraintSettings {
      public Space space = Space.Self;
      public Vector3 offsetPosition = Vector3.zero;
      public Vector3 offsetAngles = Vector3.zero;
      public Vector3 offsetScale = Vector3.one;
    }

    // コンストレイント設定
    public ConstraintSettings Settings { get; set; } = new ConstraintSettings();

    // 座標追従無効
    public bool DisablePosition { get; set; }
    // 回転追従無効
    public bool DisableRotation { get; set; }
    // 拡縮追従無効
    public bool DisableLocalScale { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ParentRuntimeConstraint(Transform owner, string targetName = "")
      : base(owner, targetName) { }

    public ParentRuntimeConstraint(Transform owner, Transform target)
      : base(owner, target) { }

    /// <summary>
    /// Transformを反映
    /// </summary>
    protected override void ApplyTransform() {
      var space = Settings.space;

      if (!DisablePosition) {
        var offsetPosition = Settings.offsetPosition;

        if (space == Space.Self) {
          offsetPosition = Owner.TransformVector(offsetPosition);
        }

        Owner.position = GetTargetPosition() + offsetPosition;
      }

      if (!DisableRotation) {
        var offsetRotation = Quaternion.Euler(Settings.offsetAngles);

        if (space == Space.Self) {
          Owner.rotation = GetTargetRotation() * offsetRotation;
        } else {
          Owner.rotation = offsetRotation * GetTargetRotation();
        }
      }

      if (!DisableLocalScale) {
        var offsetScale = Settings.offsetScale;
        Owner.localScale = Vector3.Scale(GetTargetLocalScale(), offsetScale);
      }
    }
  }
}
