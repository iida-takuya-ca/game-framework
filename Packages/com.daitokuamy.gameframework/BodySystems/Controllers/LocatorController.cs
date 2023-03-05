using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Transform管理クラス
    /// </summary>
    public class LocatorController : BodyController {
        // ロケーター管理クラスのリスト
        private List<LocatorParts> _locatorPartsList = new List<LocatorParts>();

        // Locator取得用アクセサ
        public Transform this[string key] {
            get {
                for (var i = 0; i < _locatorPartsList.Count; i++) {
                    var result = _locatorPartsList[i][key];
                    if (result != null) {
                        return result;
                    }
                }

                return Body.Transform;
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _locatorPartsList.Clear();
            _locatorPartsList.AddRange(Body.GetComponentsInChildren<LocatorParts>(true));
        }
    }
}