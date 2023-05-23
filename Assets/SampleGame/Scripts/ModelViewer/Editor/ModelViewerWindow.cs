using System;
using GameFramework.Core;
using UnityEditor;

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
            Body,
            Environment,
        }

        private class PanelBase : IDisposable {
            protected ModelViewerWindow Window { get; private set; }

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Initialize(ModelViewerWindow window) {
                
            }
            
            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            public void OnGUI() {
                
            }
        }
        
        /// <summary>
        /// 開く処理
        /// </summary>
        [MenuItem("Window/SampleGame/Model Viewer")]
        private static void Open() {
            GetWindow<ModelViewerWindow>("Model Viewer");
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
            
            
        }
    }
}