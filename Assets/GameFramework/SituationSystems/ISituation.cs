using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション定義用インターフェース
    ///
    /// Standby
    ///   LoadRoutine
    ///     Setup
    ///       OpenRoutine
    ///         Update/LateUpdate
    ///       CloseRoutine
    ///     Cleanup
    ///   Unload
    /// Release
    /// </summary>
    public interface ISituation {
        /// <summary>
        /// 待機処理
        /// </summary>
        /// <param name="parent">親Situation</param>
        void Standby(ISituation parent);
        
        /// <summary>
        /// 読み込み処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator LoadRoutine(TransitionHandle handle);
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Setup(TransitionHandle handle);
        
        /// <summary>
        /// 開く処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator OpenRoutine(TransitionHandle handle);
        
        /// <summary>
        /// 更新
        /// </summary>
        void Update();
        
        /// <summary>
        /// 後更新
        /// </summary>
        void LateUpdate();
        
        /// <summary>
        /// 閉じる処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator CloseRoutine(TransitionHandle handle);
        
        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Cleanup(TransitionHandle handle);
        
        /// <summary>
        /// 解放処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Unload(TransitionHandle handle);
        
        /// <summary>
        /// 登録解除処理
        /// </summary>
        /// <param name="parent">親Situation</param>
        void Release(ISituation parent);
    }
}