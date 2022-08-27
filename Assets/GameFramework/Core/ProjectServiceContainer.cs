#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のコンテナ
    /// </summary>
    public sealed class ProjectServiceContainer : ServiceContainer {
        // シングルトン用インスタンス
        private static IServiceContainer s_instance;
        
        // シングルトン用インスタンス取得
        public static IServiceContainer Instance {
            get {
                if (s_instance == null) {
                    s_instance = new ProjectServiceContainer();
                }

                return s_instance;
            }
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ProjectServiceContainer()
            : base(null) {
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// エディタ起動時の処理
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnInitializeOnLoad() {
            // Play/Edit切り替わり時にインスタンスを解放
            EditorApplication.playModeStateChanged += change => {
                switch (change) {
                    case PlayModeStateChange.ExitingEditMode:
                    case PlayModeStateChange.ExitingPlayMode:
                        s_instance?.Dispose();
                        s_instance = null;
                        break;
                }
            };
        }
#endif
    }
}
