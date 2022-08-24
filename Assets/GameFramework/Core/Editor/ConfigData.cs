using UnityEngine;

namespace GameFramework.Core.Editor {
    /// <summary>
    /// GameFramework用の設定データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_game_framework_config.asset", menuName = "GameFramework/Config Data")]
    public class ConfigData : ScriptableObject {
        [Tooltip("UniRxを使用するか")]
        public bool useUniRx;
    }
}
