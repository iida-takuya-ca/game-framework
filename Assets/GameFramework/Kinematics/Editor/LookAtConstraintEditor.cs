using UnityEditor;

namespace GameFramework.Kinematics.Editor {
  /// <summary>
  /// LookAtConstraint用のエディタ拡張
  /// </summary>
  [CustomEditor(typeof(LookAtConstraint))]
  public class LookAtConstraintEditor : ConstraintEditor {
    private SerializedProperty _rollProperty = null;
    private SerializedProperty _worldUpObjectProperty = null;

    /// <summary>
    /// プロパティ描画
    /// </summary>
    protected override void DrawProperties() {
      EditorGUILayout.PropertyField(_rollProperty);
      EditorGUILayout.PropertyField(_worldUpObjectProperty);
    }

    /// <summary>
    /// アクティブ時処理
    /// </summary>
    protected override void OnEnableInternal() {
      _rollProperty = serializedObject.FindProperty("_roll");
      _worldUpObjectProperty = serializedObject.FindProperty("_worldUpObject");
    }
  }
}
