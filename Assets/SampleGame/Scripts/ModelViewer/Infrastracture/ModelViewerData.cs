using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// モデルビューア用データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer.asset", menuName = "SampleGame/Model Viewer/Data")]
    public class ModelViewerData : ScriptableObject {
        [Header("Model")]
        [Tooltip("初期状態で読み込むBodyDataId")]
        public string defaultBodyDataId = "";
        [Tooltip("BodyDataのAssetKeyリスト")]
        public string[] bodyDataIds = Array.Empty<string>();

        [Header("Environment")]
        [Tooltip("初期状態で読み込む環境ID")]
        public string defaultEnvironmentId = "fld000";
        [Tooltip("EnvironmentIDリスト")]
        public string[] environmentIds = Array.Empty<string>();
    }
}
