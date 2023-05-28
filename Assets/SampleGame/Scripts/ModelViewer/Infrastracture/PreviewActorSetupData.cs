using System;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// プレビュー用のActorデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_preview_actor_setup_hoge.asset", menuName = "SampleGame/Model Viewer/Preview Actor Setup Data")]
    public class PreviewActorSetupData : ScriptableObject {
        [Tooltip("Body用のPrefab")]
        public GameObject prefab;
        [Tooltip("アニメーションクリップリスト")]
        public AnimationClip[] animationClips = Array.Empty<AnimationClip>();
    }
}
