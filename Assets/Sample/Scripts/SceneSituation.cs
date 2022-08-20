using System.Linq;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移に使うシチュエーション
/// </summary>
public abstract class SceneSituation : Situation {
    // シーンのアセットパス
    public abstract string SceneAssetPath { get; }
    // シーン情報
    protected Scene Scene { get; private set; }

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void SetupInternal(TransitionHandle handle, IScope scope) {
        // シーンの取得
        Scene = SceneManager.GetActiveScene();
        
        // Serviceのインストール
        var installers = Scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<ServiceLocatorInstaller>(true))
            .ToArray();
        foreach (var installer in installers) {
            installer.Install(ServiceLocator);
        }
    }
}