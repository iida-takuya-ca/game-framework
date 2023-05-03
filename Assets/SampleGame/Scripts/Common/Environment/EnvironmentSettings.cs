using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 環境設定
    /// </summary>
    [ExecuteAlways]
    public class EnvironmentSettings : MonoBehaviour {
        [SerializeField, Tooltip("反映対象のデータ")]
        private EnvironmentContextData _data;
        [SerializeField, Tooltip("平行光源")]
        private Light _sun;
        [SerializeField, Tooltip("ブレンド時間")]
        private float _blendDuration;

        private EnvironmentHandle _handle;

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            if (_data == null) {
                return;
            }

            var manager = Services.Get<EnvironmentManager>();
            if (manager != null) {
                var context = _data.CreateContext();
                context.Sun = _sun;
                _handle = manager.Push(context, _blendDuration);
            }

            if (_sun != null) {
                _sun.enabled = true;
            }
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            if (_handle.IsValid) {
                var manager = Services.Get<EnvironmentManager>();
                if (manager != null) {
                    manager.Remove(_handle);
                }
            }

            if (_sun != null) {
                _sun.enabled = false;
            }
        }
    }
}