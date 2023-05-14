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

        [Header("Field")]
        [Tooltip("初期状態で読み込むFieldのID")]
        public string defaultFieldId = "fld000";
        [Tooltip("FieldのIDリスト")]
        public string[] fieldIds = Array.Empty<string>();
    }
}
