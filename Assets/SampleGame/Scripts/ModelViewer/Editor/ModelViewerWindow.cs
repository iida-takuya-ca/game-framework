using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEditor;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// モデルビューア用のWindow
    /// </summary>
    public partial class ModelViewerWindow : EditorWindow {
        /// <summary>
        /// パネルのタイプ
        /// </summary>
        private enum PanelType {
            Model,
            Component,
            Environment,
        }

        /// <summary>
        /// タブ毎に描画する内容の規定
        /// </summary>
        private abstract class PanelBase : IDisposable {
            private DisposableScope _scope;
            private Vector2 _scroll;

            protected ModelViewerWindow Window { get; private set; }

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Initialize(ModelViewerWindow window) {
                _scope = new();
                Window = window;
                InitializeInternal(_scope);
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                DisposeInternal();
                _scope.Dispose();
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            public void OnGUI() {
                using (var scope = new EditorGUILayout.ScrollViewScope(_scroll)) {
                    OnGUIInternal();
                    _scroll = scope.scrollPosition;
                }
            }

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected virtual void InitializeInternal(IScope scope) {
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            protected virtual void DisposeInternal() {
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected abstract void OnGUIInternal();
        }

        private Dictionary<PanelType, PanelBase> _panels = new();
        private PanelType _currentPanelType;

        /// <summary>
        /// 開く処理
        /// </summary>
        [MenuItem("Window/SampleGame/Model Viewer")]
        private static void Open() {
            GetWindow<ModelViewerWindow>("Model Viewer");
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            // パネルの生成
            void CreatePanel<T>(PanelType panelType)
                where T : PanelBase, new() {
                if (_panels.ContainsKey(panelType)) {
                    return;
                }
                
                var panel = new T();
                _panels[panelType] = panel;
            }
            
            CreatePanel<ModelPanel>(PanelType.Model);
            CreatePanel<ComponentPanel>(PanelType.Component);
            CreatePanel<EnvironmentPanel>(PanelType.Environment);
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            foreach (var pair in _panels) {
                pair.Value.Dispose();
            }

            _panels.Clear();
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            var modelViewerModel = ModelViewerModel.Get();
            if (modelViewerModel == null) {
                EditorGUILayout.HelpBox("Not found ModelViewerManager", MessageType.Error);
                return;
            }

            var labels = Enum.GetNames(typeof(PanelType));
            _currentPanelType = (PanelType)GUILayout.Toolbar((int)_currentPanelType, labels, EditorStyles.toolbarButton);
            var panel = GetPanel(_currentPanelType);
            if (panel != null) {
                panel.OnGUI();
            }
        }

        /// <summary>
        /// パネルの取得
        /// </summary>
        private PanelBase GetPanel(PanelType type) {
            _panels.TryGetValue(type, out var panel);
            return panel;
        }
    }
}