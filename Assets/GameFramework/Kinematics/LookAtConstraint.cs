using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Kinematics {
  /// <summary>
  /// 注視コンストレイント
  /// </summary>
  public class LookAtConstraint : Constraint {
    // コンストレイント設定
    [Serializable]
    public class ConstraintSettings {
      public Space space = Space.Self;
      public Vector3 offsetAngles = Vector3.zero;
    }

    [SerializeField, Tooltip("コンストレイント設定")]
    private ConstraintSettings _settings = null;
    [SerializeField, Tooltip("ねじり角度")]
    private float _roll = 0.0f;
    [SerializeField, Tooltip("UpベクトルをさすTransform(未指定はデフォルト)")]
    private Transform _worldUpObject = null;

    // コンストレイント設定
    public ConstraintSettings Settings {
      get => _settings;
      set => _settings = value;
    }
    // ねじり角度
    public float Roll {
      set { _roll = value; }
    }
    // UpベクトルをさすTransform
    public Transform WorldUpObject {
      set { _worldUpObject = value; }
    }

    /// <summary>
    /// 自身のTransformからオフセットを設定する
    /// </summary>
    public override void TransferOffset() {
      var space = _settings.space;
      var upVector = _worldUpObject != null ? _worldUpObject.up : Vector3.up;
      var baseRotation =
        Quaternion.LookRotation(GetTargetPosition() - transform.position, upVector) *
        Quaternion.Euler(0.0f, 0.0f, _roll);

      Quaternion offsetRotation;

      if (space == Space.Self) {
        offsetRotation = Quaternion.Inverse(baseRotation) * transform.rotation;
      } else {
        offsetRotation = transform.rotation * Quaternion.Inverse(baseRotation);
      }

      _settings.offsetAngles = offsetRotation.eulerAngles;
    }

    /// <summary>
    /// オフセットを初期化
    /// </summary>
    public override void ResetOffset() {
      _settings.offsetAngles = Vector3.zero;
    }

    /// <summary>
    /// Transformを反映
    /// </summary>
    public override void ApplyTransform() {
      var space = _settings.space;
      var offsetRotation = Quaternion.Euler(_settings.offsetAngles);
      var upVector = _worldUpObject != null ? _worldUpObject.up : Vector3.up;
      var baseRotation =
        Quaternion.LookRotation(GetTargetPosition() - transform.position, upVector) *
        Quaternion.Euler(0.0f, 0.0f, _roll);

      if (space == Space.Self) {
        transform.rotation = baseRotation * offsetRotation;
      } else {
        transform.rotation = offsetRotation * baseRotation;
      }
    }
  }
}
