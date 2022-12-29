using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 環境設定用のアセット
    /// </summary>
    [CreateAssetMenu(fileName = "dat_env_hoge.asset", menuName = "Sample/Environment Context Data")]
    public class EnvironmentContextData : ScriptableObject, IEnvironmentContext {
        [SerializeField]
        public EnvironmentDefaultSettings defaultSettings;

        public EnvironmentDefaultSettings DefaultSettings => defaultSettings;
    }
}