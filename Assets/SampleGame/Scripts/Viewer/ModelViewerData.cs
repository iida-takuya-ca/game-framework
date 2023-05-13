using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// モデルビューア用データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer.asset", menuName = "SampleGame/Model Viewer/Data")]
    public class ModelViewerData : ScriptableObject {
        [Tooltip("初期状態で読み込むBodyDataId")]
        public string defaultBodyDataId = "";
        [Tooltip("BodyDataのAssetKeyリスト")]
        public string[] bodyDataIds = Array.Empty<string>();
    }
}
