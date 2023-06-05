using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UniRx;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// ModelViewerのAvatarパネル
    /// </summary>
    partial class ModelViewerWindow {
        /// <summary>
        /// AvatarPanel
        /// </summary>
        private class AvatarPanel : PanelBase {
            private PreviewActor _actor;
            private Dictionary<string, FoldoutList<GameObject>> _meshAvatarFoldoutLists = new();
            private Dictionary<string, GameObject[]> _meshAvatarPrefabLists = new();
            
            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
                var entityManager = Services.Get<ActorManager>();
                entityManager.PreviewActor
                    .TakeUntil(scope)
                    .Subscribe(x => {
                        _actor = x;
                        
                        _meshAvatarFoldoutLists.Clear();
                        _meshAvatarPrefabLists.Clear();
                        if (x != null) {
                            foreach (var info in x.SetupData.meshAvatarInfos) {
                                var list = new FoldoutList<GameObject>(info.key);
                                _meshAvatarFoldoutLists[info.key] = list;
                                _meshAvatarPrefabLists[info.key] = info.prefabs.Concat(new GameObject[] { null }).ToArray();
                            }
                        }
                    });
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                foreach (var pair in _meshAvatarFoldoutLists) {
                    var list = pair.Value;
                    var prefabs = _meshAvatarPrefabLists[pair.Key];
                    list.OnGUI(prefabs, (prefab, index) => {
                        if (prefab != null) {
                            if (GUILayout.Button(prefab.name)) {
                                _actor.ChangeMeshAvatar(pair.Key, index);
                            }
                        }
                        else {
                            if (GUILayout.Button("Remove")) {
                                _actor.ChangeMeshAvatar(pair.Key, -1);
                            }
                        }
                    });
                }
            }
        }
    }
}