using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using UnityEngine;

/// <summary>
/// 環境設定
/// </summary>
[ExecuteAlways]
public class EnvironmentSettings : MonoBehaviour {
    [SerializeField, Tooltip("反映対象のデータ")]
    private EnvironmentContextData _data;
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
            _handle = manager.Push(_data, _blendDuration);
        }
    }

    /// <summary>
    /// 非アクティブ時処理
    /// </summary>
    private void OnDisable() {
        if (!_handle.IsValid) {
            return;
        }
        
        var manager = Services.Get<EnvironmentManager>();
        if (manager != null) {
            manager.Remove(_handle);
        }
    }
}