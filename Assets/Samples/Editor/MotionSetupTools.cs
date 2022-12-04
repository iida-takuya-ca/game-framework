using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// モーション初期化ツール
/// </summary>
public static class MotionSetupTools
{
    /// <summary>
    /// モーションの初期化
    /// </summary>
    [MenuItem("Assets/Sample/Setup Motion")]
    private static void SetupMotion() {
        var targets = Selection.GetFiltered<GameObject>(SelectionMode.DeepAssets);
        foreach (var target in targets) {
            try {
                SetupMotion(target);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
    }

    /// <summary>
    /// モーションの初期化
    /// </summary>
    private static void SetupMotion(GameObject target) {
        var type = PrefabUtility.GetPrefabAssetType(target);
        if (type != PrefabAssetType.Model) {
            return;
        }

        var path = AssetDatabase.GetAssetPath(target);
        
        // Importerの取得
        var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
        if (modelImporter == null) {
            return;
        }

        // TakeNameをFBXの名前とそろえる
        var baseName = target.name;
        var clipAnimations = modelImporter.clipAnimations;
        var singleClip = clipAnimations.Length == 1;
        for (var i = 0; i < clipAnimations.Length; i++) {
            var clipAnimation = clipAnimations[i];
            var suffix = singleClip ? "" : $"_{i:00}";
            clipAnimation.takeName = baseName + suffix;
            clipAnimation.name = clipAnimation.takeName;
        }
        
        // Materialは全てOFF
        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
        
        // Meshの設定も基本削除
        modelImporter.meshCompression = ModelImporterMeshCompression.Off;
        modelImporter.meshOptimizationFlags = 0;
        modelImporter.optimizeMeshPolygons = false;
        modelImporter.optimizeMeshVertices = false;
        modelImporter.importNormals = ModelImporterNormals.None;
        
        // Avatarの設定
        modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        modelImporter.clipAnimations = clipAnimations;
        modelImporter.SaveAndReimport();
    }
}
