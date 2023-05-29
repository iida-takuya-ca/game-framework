using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
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
            Actor,
            Body,
            Avatar,
            Environment,
            Settings,
        }

        /// <summary>
        /// 検索可能なリストGUI
        /// </summary>
        private class SearchableList<T> {
            private SearchField _searchField;
            private string _filter;
            private Vector2 _scroll;
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public SearchableList() {
                _searchField = new SearchField();
                _filter = "";
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            /// <param name="items">表示項目</param>
            /// <param name="itemToName">項目名変換関数</param>
            /// <param name="onGUIElement">項目GUI描画</param>
            /// <param name="options">LayoutOption</param>
            public void OnGUI(IList<T> items, Func<T, string> itemToName, Action<T, int> onGUIElement, params GUILayoutOption[] options) {
                using (new EditorGUILayout.VerticalScope("Box", options)) {
                    // 検索フィルタ
                    _filter = _searchField.OnToolbarGUI(_filter);
                    var filteredItems = items
                        .Where(x => {
                            if (string.IsNullOrEmpty(_filter)) {
                                return true;
                            }

                            var splitFilters = _filter.Split(" ");
                            var name = itemToName.Invoke(x);
                            foreach (var filter in splitFilters) {
                                if (!name.Contains(filter)) {
                                    return false;
                                }
                            }

                            return true;
                        })
                        .ToArray();

                    // 項目描画
                    using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, "Box")) {
                        for (var i = 0; i < filteredItems.Length; i++) {
                            onGUIElement.Invoke(filteredItems[i], i);
                        }

                        _scroll = scope.scrollPosition;
                    }
                }
            }
        }

        /// <summary>
        /// 検索可能なリストGUI
        /// </summary>
        private class FoldoutList<T> {
            private readonly string _label;
            private bool _open;
            private Vector2 _scroll;
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public FoldoutList(string label, bool defaultOpen = true) {
                _label = label;
                _open = defaultOpen;
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            /// <param name="items">表示項目</param>
            /// <param name="onGUIElement">項目GUI描画</param>
            /// <param name="options">LayoutOption</param>
            public void OnGUI(IList<T> items, Action<T, int> onGUIElement, params GUILayoutOption[] options) {
                _open = EditorGUILayout.ToggleLeft(_label, _open, EditorStyles.boldLabel);
                if (_open) {
                    // 項目描画
                    using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, "Box", options)) {
                        for (var i = 0; i < items.Count; i++) {
                            onGUIElement.Invoke(items[i], i);
                        }

                        _scroll = scope.scrollPosition;
                    }
                }
            }
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
        [MenuItem("Window/Sample Game/Model Viewer")]
        private static void Open() {
            GetWindow<ModelViewerWindow>("Model Viewer");
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            ClearPanels();
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("Playing Mode Only", MessageType.Error);
                ClearPanels();
                return;
            }
            
            var modelViewerModel = ModelViewerModel.Get();
            if (modelViewerModel == null) {
                EditorGUILayout.HelpBox("Not found ModelViewerModel", MessageType.Error);
                ClearPanels();
                return;
            }

            CreatePanels();

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

        /// <summary>
        /// パネルの生成
        /// </summary>
        private void CreatePanels() {
            // パネルの生成
            void CreatePanel<T>(PanelType panelType)
                where T : PanelBase, new() {
                if (_panels.ContainsKey(panelType)) {
                    return;
                }
                
                var panel = new T();
                panel.Initialize(this);
                _panels[panelType] = panel;
            }
            
            CreatePanel<ActorPanel>(PanelType.Actor);
            CreatePanel<BodyPanel>(PanelType.Body);
            CreatePanel<AvatarPanel>(PanelType.Avatar);
            CreatePanel<EnvironmentPanel>(PanelType.Environment);
            CreatePanel<SettingsPanel>(PanelType.Settings);
        }

        /// <summary>
        /// パネルのクリア
        /// </summary>
        private void ClearPanels() {
            foreach (var pair in _panels) {
                pair.Value.Dispose();
            }

            _panels.Clear();
        }
    }
}