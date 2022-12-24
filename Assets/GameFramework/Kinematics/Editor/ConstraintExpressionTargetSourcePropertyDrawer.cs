using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Kinematics.Editor {
    /// <summary>
    /// Constrant.TargetSourceのドローアー
    /// </summary>
    [CustomPropertyDrawer(typeof(ConstraintExpression.TargetSource))]
    public class ConstraintExpressionTargetSourcePropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var target = property.FindPropertyRelative("target");
            var targetPath = property.FindPropertyRelative("targetPath");
            var weight = property.FindPropertyRelative("weight");

            var leftRect = position;
            var rightRect = position;
            leftRect.xMax = position.xMax - 100;
            rightRect.xMin = leftRect.xMax;
            target.objectReferenceValue = EditorGUI.ObjectField(leftRect, target.objectReferenceValue,
                typeof(Transform), true);
            weight.floatValue = Mathf.Max(EditorGUI.FloatField(rightRect, weight.floatValue), 0.0f);
        }

        /// <summary>
        /// GUI描画領域の高さ計算
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Transformの相対パスを取得
        /// </summary>
        private string GetRelativePath(Transform root, Transform target) {
            var rootPath = GetHierarchyPath(root);
            var targetPath = GetHierarchyPath(target);

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
                relativePath += $"/{splitRootPaths[i]}";
            }

            return relativePath;
        }

        /// <summary>
        /// ヒエラルキー上のパスを取得
        /// </summary>
        private string GetHierarchyPath(Transform target) {
            var path = target.gameObject.name;
            var parent = target.parent;

            while (parent != null) {
                path = $"{parent.gameObject.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }
    }
}