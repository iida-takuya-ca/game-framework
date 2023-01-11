using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// モデルビューア用データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer.asset", menuName = "SampleGame/Model Viewer/Data")]
    public class ModelViewerData : ScriptableObject {
        [Tooltip("BodyDataのAssetKeyリスト")]
        public string[] bodyDataIds = Array.Empty<string>();
    }
}
