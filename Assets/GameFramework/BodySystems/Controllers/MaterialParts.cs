using System;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Mesh結合情報保持クラス
    /// </summary>
    [DisallowMultipleComponent]
    public class MaterialParts : MonoBehaviour {
        /// <summary>
        /// 登録情報
        /// </summary>
        [Serializable]
        public class Info {
            [Tooltip("Materialを取得する際のキー")] public string key;

            [Tooltip("MaterialがアサインされているRenderer")]
            public Renderer renderer;

            [Tooltip("対象のMaterial")] public Material material;
        }

        [Tooltip("マテリアル登録情報")] public Info[] infos;
    }
}