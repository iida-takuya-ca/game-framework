using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body用のユーティリティ
    /// </summary>
    public static class BodyUtility {
        /// <summary>
        /// GameObjectの廃棄
        /// </summary>
        public static void Destroy(GameObject gameObject) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Object.DestroyImmediate(gameObject);
                return;
            }
#endif
            Object.Destroy(gameObject);
        }
        
        /// <summary>
        /// Componentの廃棄
        /// </summary>
        public static void Destroy(Component component) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Object.DestroyImmediate(component);
                return;
            }
#endif
            Object.Destroy(component);
        }
    }
}