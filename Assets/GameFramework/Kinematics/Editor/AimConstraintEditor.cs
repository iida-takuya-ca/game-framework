using UnityEditor;

namespace GameFramework.Kinematics.Editor {
    /// <summary>
    /// ParentConstraint用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(AimConstraint))]
    public class AimConstraintEditor : ConstraintEditor {
        private SerializedProperty _forwardVectorProperty = null;
        private SerializedProperty _upVectorProperty = null;
        private SerializedProperty _worldUpObjectProperty = null;

        /// <summary>
        /// プロパティ描画
        /// </summary>
        protected override void DrawProperties() {
            EditorGUILayout.PropertyField(_forwardVectorProperty);
            EditorGUILayout.PropertyField(_upVectorProperty);
            EditorGUILayout.PropertyField(_worldUpObjectProperty);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void OnEnableInternal() {
            _forwardVectorProperty = serializedObject.FindProperty("_forwardVector");
            _upVectorProperty = serializedObject.FindProperty("_upVector");
            _worldUpObjectProperty = serializedObject.FindProperty("_worldUpObject");
        }
    }
}