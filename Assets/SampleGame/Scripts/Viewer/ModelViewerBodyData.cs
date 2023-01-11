using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// モデルビューア用のBody参照データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_body_hoge.asset", menuName = "SampleGame/Model Viewer/Body Data")]
    public class ModelViewerBodyData : ScriptableObject {
        [Tooltip("Body用のPrefab")]
        public GameObject prefab;
        [Tooltip("アニメーションクリップリスト")]
        public AnimationClip[] animationClips = Array.Empty<AnimationClip>();
    }
}
