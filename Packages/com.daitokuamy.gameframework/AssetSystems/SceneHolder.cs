#if USE_ADDRESSABLES
using UnityEngine.ResourceManagement.ResourceProviders;

#else
using UnityEngine.SceneManagement;
#endif

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーン情報保持用の構造体
    /// </summary>
    public struct SceneHolder {
#if USE_ADDRESSABLES
        public SceneInstance Scene;
#else
        public Scene Scene;
#endif
    }
}