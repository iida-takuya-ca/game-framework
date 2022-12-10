using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Transform管理クラス
    /// </summary>
    public class LocatorParts : MonoBehaviour {
        // ロケーター情報
        [Serializable]
        private class LocatorInfo {
            public string key;
            public Transform transform;
        }

        [SerializeField, Tooltip("Locatlr情報")] private LocatorInfo[] _locatorInfos = new LocatorInfo[0];

        // ロケーター情報
        private Dictionary<string, Transform> _locators;

        // ロケーター情報のアクセサ
        public Transform this[string key] {
            get {
                if (_locators == null) {
                    _locators = _locatorInfos.ToDictionary(x => x.key, x => x.transform);
                }

                if (_locators.TryGetValue(key, out var result)) {
                    return result;
                }

                return null;
            }
        }
    }
}