using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Core.Editor {
    /// <summary>
    /// ConfigData用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(ConfigData))]
    public class ConfigDataEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ用のGUI描画
        /// </summary>
        public override void OnInspectorGUI() {
            var configData = target as ConfigData;
            serializedObject.Update();

            var useUniRxProp = serializedObject.FindProperty(nameof(configData.useUniRx));
            EditorGUILayout.PropertyField(useUniRxProp);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Apply")) {
                Apply(configData);
            }
        }

        /// <summary>
        /// 値の反映
        /// </summary>
        public void Apply(ConfigData configData) {
            ApplyDefineSymbol("USE_UNI_RX", configData.useUniRx);
        }

        /// <summary>
        /// DefineSymbolの適用
        /// </summary>
        /// <param name="symbol">対象のシンボル名</param>
        /// <param name="on">シンボルを使うか</param>
        private void ApplyDefineSymbol(string symbol, bool on) {
            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            // 現在のSymbol取得
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var symbolList = symbols
                .Replace(" ", string.Empty)
                .Split(';')
                .ToList();

            // Symbolの追加/削除
            if (on) {
                if (symbolList.Contains(symbol)) {
                    return;
                }

                symbolList.Add(symbol);
            }
            else {
                if (!symbolList.Contains(symbol)) {
                    return;
                }

                symbolList.Remove(symbol);
            }

            // Symbolの再反映
            symbols = string.Join(";", symbolList);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, symbols);
        }
    }
}