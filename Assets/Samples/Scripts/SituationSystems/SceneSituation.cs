using System.Collections;
using System.Linq;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;
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
    /// 読み込み処理
    /// </summary>
    protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
        // シーンの切り替え
        yield return SceneManager.LoadSceneAsync(SceneAssetPath, LoadSceneMode.Single);

        // シーンの取得
        Scene = SceneManager.GetActiveScene();

        if (Scene.path != SceneAssetPath && Scene.name != SceneAssetPath) {
            Debug.LogError($"Failed load scene. [{SceneAssetPath}]");
        }

        // Serviceのインストール
        var installers = Scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<ServiceLocatorInstaller>(true))
            .ToArray();
        foreach (var installer in installers) {
            installer.Install(ServiceContainer);
        }
    }
}