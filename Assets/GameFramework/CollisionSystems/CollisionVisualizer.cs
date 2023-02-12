using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョン描画クラス
    /// </summary>
    public class CollisionVisualizer : MonoBehaviour {
        private static CollisionVisualizer s_instance;

        private List<ICollision> _collisions = new List<ICollision>();

        // Singletonインスタンス
        private static CollisionVisualizer Instance {
            get {
                if (!Application.isPlaying) {
                    return null;
                }
                
                if (s_instance == null) {
                    var gameObj = new GameObject(nameof(CollisionVisualizer), typeof(CollisionVisualizer));
                    DontDestroyOnLoad(gameObj);
                    s_instance = gameObj.GetComponent<CollisionVisualizer>();
                }

                return s_instance;
            }
        }
        
        /// <summary>
        /// 描画登録
        /// </summary>
        public static void Register(ICollision collision) {
#if !UNITY_EDITOR
            return;
#endif
            Instance._collisions.Add(collision);
        }

        /// <summary>
        /// 描画登録解除
        /// </summary>
        public static void Unregister(ICollision collision) {
#if !UNITY_EDITOR
            return;
#endif
            Instance._collisions.Remove(collision);
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        private void OnDrawGizmos() {
            foreach (var collision in _collisions) {
                collision.DrawGizmos();
            }
        }
    }
}