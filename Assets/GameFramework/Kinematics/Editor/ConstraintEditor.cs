using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameFramework.Kinematics.Editor {
    /// <summary>
    /// Constraint用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(Constraint), true)]
    public class ConstraintEditor : UnityEditor.Editor {
        private ReorderableList _sourceList = null;
        private SerializedProperty _activeProperty = null;
        private SerializedProperty _updateModeProperty = null;
        private SerializedProperty _sourcesProperty = null;
        private SerializedProperty _settingsProperty = null;

        private bool _lock = false;

        /// <summary>
        /// インスペクタGUI描画
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawGUI();

            serializedObject.ApplyModifiedProperties();

            // 変更があった場合、パスの反映
            if (GUI.changed) {
                ApplyTargetPath();
            }
        }

        /// <summary>
        /// Transform情報のOffset転送
        /// </summary>
        protected virtual void TransferOffset() {
            var constraint = (Constraint)target;
            // 現在のTransformで設定を更新
            constraint.TransferOffset();
            constraint.ApplyTransform();
            serializedObject.Update();
        }

        /// <summary>
        /// Offsetのゼロクリア
        /// </summary>
        protected virtual void ResetOffset() {
            var constraint = (Constraint)target;
            constraint.ResetOffset();
            constraint.ApplyTransform();
            serializedObject.Update();
        }

        /// <summary>
        /// 設定項目の描画
        /// </summary>
        protected virtual void DrawProperties() {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected virtual void OnEnableInternal() {
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected virtual void OnDisableInternal() {
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void DrawGUI() {
            // 更新モード
            EditorGUILayout.PropertyField(_updateModeProperty);

            EditorGUILayout.Space();

            // 有効化ボタン
            var prevColor = GUI.color;
            GUI.color = _activeProperty.boolValue ? Color.cyan : prevColor;

            if (GUILayout.Button(_activeProperty.boolValue ? " Active " : "Activate")) {
                _activeProperty.boolValue = !_activeProperty.boolValue;

                if (_activeProperty.boolValue) {
                    // アクティブ時にロックする
                    _lock = true;
                }
            }

            GUI.color = prevColor;

            using (new EditorGUILayout.HorizontalScope()) {
                // 転送ボタン
                using (new EditorGUI.DisabledScope(_lock)) {
                    if (GUILayout.Button("Transfer")) {
                        if (!_lock) {
                            TransferOffset();
                        }
                    }
                }

                // リセットボタン
                using (new EditorGUI.DisabledScope(_lock)) {
                    if (GUILayout.Button("Zero")) {
                        ResetOffset();
                    }
                }
            }

            // ロック
            _lock = EditorGUILayout.ToggleLeft("Lock", _lock);

            using (new EditorGUI.DisabledScope(_lock)) {
                // 設定描画
                EditorGUILayout.PropertyField(_settingsProperty, true);
                // プロパティ描画
                DrawProperties();

                // ターゲット情報描画
                _sourceList.DoLayoutList();
            }
        }

        /// <summary>
        /// パスの反映
        /// </summary>
        private void ApplyTargetPath() {
            var rootTransform = ((Constraint)target).transform;

            serializedObject.Update();

            for (var i = 0; i < _sourcesProperty.arraySize; i++) {
                var sourceProp = _sourcesProperty.GetArrayElementAtIndex(i);
                var targetProp = sourceProp.FindPropertyRelative("target");
                var targetPathProp = sourceProp.FindPropertyRelative("targetPath");
                // 相対パスの反映
                targetPathProp.stringValue =
                    GetRelativePath(rootTransform, targetProp.objectReferenceValue as Transform);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Transformの相対パスを取得
        /// </summary>
        private string GetRelativePath(Transform rootTransform, Transform targetTransform) {
            if (rootTransform == null || targetTransform == null) {
                return "";
            }

            var rootPath = GetHierarchyPath(rootTransform);
            var targetPath = GetHierarchyPath(targetTransform);

            // 相対パスに変換
            var splitRootPaths = rootPath.Split('/');
            var splitTargetPaths = targetPath.Split('/');
            var branchPathIndex = 0;

            // 分岐点を取得
            for (var i = 0; i < splitRootPaths.Length; i++) {
                if (i >= splitTargetPaths.Length) {
                    break;
                }

                if (splitRootPaths[i] != splitTargetPaths[i]) {
                    break;
                }

                branchPathIndex++;
            }

            var parentOffset = splitRootPaths.Length - branchPathIndex;
            var relativePath = string.Concat(Enumerable.Repeat("../", parentOffset));

            // 分岐点以降を結合
            for (var i = branchPathIndex; i < splitTargetPaths.Length; i++) {
                relativePath += splitTargetPaths[i];

                if (i < splitTargetPaths.Length - 1) {
                    relativePath += "/";
                }
            }

            return relativePath;
        }

        /// <summary>
        /// ヒエラルキー上のパスを取得
        /// </summary>
        private string GetHierarchyPath(Transform targetTransform) {
            var path = targetTransform.gameObject.name;
            var parent = targetTransform.parent;

            while (parent != null) {
                path = $"{parent.gameObject.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }

        /// <summary>
        /// リストの要素描画コールバック
        /// </summary>
        private void OnDrawHeader(Rect rect) {
            EditorGUI.LabelField(rect, "Source List", EditorStyles.boldLabel);
        }

        /// <summary>
        /// リストの要素描画コールバック
        /// </summary>
        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused) {
            var element = _sourcesProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element);
        }

        /// <summary>
        /// リストの要素高さ取得コールバック
        /// </summary>
        private float OnElementHeight(int index) {
            var element = _sourcesProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _activeProperty = serializedObject.FindProperty("_active");
            _updateModeProperty = serializedObject.FindProperty("_updateMode");
            _sourcesProperty = serializedObject.FindProperty("_sources");
            _settingsProperty = serializedObject.FindProperty("_settings");

            _sourceList = new ReorderableList(serializedObject, _sourcesProperty);
            _sourceList.drawHeaderCallback = OnDrawHeader;
            _sourceList.drawElementCallback = OnDrawElement;
            _sourceList.elementHeightCallback = OnElementHeight;

            OnEnableInternal();
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            if (_sourceList != null) {
                _sourceList.drawHeaderCallback = null;
                _sourceList.drawElementCallback = null;
                _sourceList.elementHeightCallback = null;
            }

            OnDisableInternal();
        }
    }
}