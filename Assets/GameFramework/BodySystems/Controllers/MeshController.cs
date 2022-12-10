using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Mesh制御クラス
    /// </summary>
    public class MeshController : BodyController {
        // 追加したメッシュに関係する情報
        private class MergedInfo {
            // 追加した骨のリスト
            public Transform[] additiveBones;
            // メッシュパーツ
            public MeshParts meshParts;
        }

        // メッシュを追加する場所
        private Transform _additiveMeshRoot;
        // 追加したメッシュの情報
        private Dictionary<GameObject, MergedInfo> _mergedInfos =
            new Dictionary<GameObject, MergedInfo>();
        // 骨の参照カウンタ保持用
        private Dictionary<Transform, int> _boneReferenceCounts = new Dictionary<Transform, int>();

        // 更新通知
        public event Action OnRefreshed;
        // Partsの追加通知
        public event Action<GameObject> OnAddedParts;
        // Partsの削除通知
        public event Action<GameObject> OnRemovedParts;
        // MeshPartsの追加通知
        public event Action<MeshParts> OnAddedMeshParts;
        // MeshPartsの削除通知
        public event Action<MeshParts> OnRemovedMeshParts;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // 追加Mesh用の箱を作る
            _additiveMeshRoot = new GameObject("AdditiveMeshRoot").transform;
            _additiveMeshRoot.gameObject.layer = Body.GameObject.layer;
            _additiveMeshRoot.SetParent(Body.Transform, false);
        }

        /// <summary>
        /// GameObjectのMeshをマージする
        /// </summary>
        /// <param name="target">Meshを含むGameObject</param>
        /// <param name="prefix">骨をマージする際につけるPrefix</param>
        public void MergeMeshes(GameObject target, string prefix) {
            // 追加済み
            if (_mergedInfos.ContainsKey(target)) {
                Debug.LogWarning($"Already merged target. [{target.name}]");
                return;
            }

            // 骨のマージ
            var additiveBones = MergeBones(target, prefix);

            // ターゲットを移動させる
            target.transform.SetParent(_additiveMeshRoot, false);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;

            // Layerを同じにする
            BodyUtility.SetLayer(target, _additiveMeshRoot.gameObject.layer);

            // メッシュ情報をキャッシュ
            var meshParts = target.GetComponent<MeshParts>();
            _mergedInfos[target] = new MergedInfo {
                additiveBones = additiveBones,
                meshParts = meshParts,
            };

            // 骨参照カウントの更新
            foreach (var bone in additiveBones) {
                _boneReferenceCounts.TryGetValue(bone, out var referenceCount);
                referenceCount++;
                _boneReferenceCounts[bone] = referenceCount;
            }

            // 更新通知
            if (meshParts != null) {
                OnAddedMeshParts?.Invoke(meshParts);
            }

            OnAddedParts?.Invoke(target);
            OnRefreshed?.Invoke();
        }

        /// <summary>
        /// Merge済みのMeshを削除する
        /// </summary>
        /// <param name="target">Merge済みなGameObject</param>
        public void DeleteMergedMeshes(GameObject target) {
            if (!_mergedInfos.TryGetValue(target, out var mergedInfo)) {
                Debug.LogWarning($"Not merged target. [{target.name}]");
                return;
            }

            // 骨の削除
            DeleteBones(mergedInfo);
            // キャッシュクリア
            _mergedInfos.Remove(target);

            // 削除通知
            if (mergedInfo.meshParts != null) {
                OnRemovedMeshParts?.Invoke(mergedInfo.meshParts);
            }

            OnRemovedParts?.Invoke(target);
            // 追加したメッシュの削除
            UnityEngine.Object.DestroyImmediate(target.gameObject);
            // 更新通知
            OnRefreshed?.Invoke();
        }

        /// <summary>
        /// 所持しているMeshPartsのリスト
        /// </summary>
        public MeshParts[] GetMeshPartsList() {
            return _mergedInfos.Select(x => x.Value.meshParts).ToArray();
        }

        /// <summary>
        /// 骨のマージ
        /// </summary>
        /// <param name="target">対象のGameObject</param>
        /// <param name="prefix">マージする際につけるPrefix</param>
        private Transform[] MergeBones(GameObject target, string prefix) {
            var boneController = Body.GetController<BoneController>();

            if (boneController == null || boneController.Root == null) {
                return Array.Empty<Transform>();
            }

            // メッシュパーツ情報
            var meshParts = target.GetComponent<MeshParts>();

            // 既に存在する骨の列挙
            var currentBones = boneController.Root.GetComponentsInChildren<Transform>()
                .ToDictionary(x => x.name, x => x);

            // 対象の情報取得
            var root = target.transform.Find(boneController.Root.name);
            var rootName = root.name;
            var renderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            // Mergeできない
            if (root == null || renderers.Length <= 0) {
                return Array.Empty<Transform>();
            }

            var bones = root.GetComponentsInChildren<Transform>(true);

            // ユニーク骨を他のMerge対象と被らせないようにするために置き換え
            if (meshParts != null) {
                ConvertUniqueBoneName(meshParts.uniqueBoneInfos.SelectMany(x => x.targetBones).ToArray(),
                    prefix);
            }

            // 関節を列挙
            var addableBones = new List<Transform>();
            var deletableBones = new List<Transform>();
            var addedBones = new List<Transform>();

            // 既存の骨に含まれている物とそうでないものを分離
            for (var i = bones.Length - 1; i >= 0; i--) {
                var bone = bones[i];

                if (bone == null) {
                    continue;
                }

                if (currentBones.TryGetValue(bone.name, out var currentBone)) {
                    deletableBones.Add(bone);

                    // 別で追加された骨の場合は覚えておく(参照カウンタUP用)
                    if (_boneReferenceCounts.ContainsKey(currentBone)) {
                        addedBones.Add(currentBone);
                    }
                }
                else {
                    addableBones.Add(bone);
                }
            }

            // 骨の追加
            foreach (var addBone in addableBones) {
                if (currentBones.TryGetValue(addBone.parent.name, out var parentBone)) {
                    addBone.SetParent(parentBone, false);
                }
            }

            // Rendererの中身を入れ替える
            var targetBones = new List<Transform>();

            // 対応するCurrentBoneの取得
            Transform GetCurrentBone(Transform targetBone) {
                // null骨だった場合、直下の骨名の親を置き換え対象にする
                if (targetBone.name == rootName) {
                    if (targetBone.childCount > 0) {
                        var childBone = targetBone.GetChild(0);
                        var bone = GetCurrentBone(childBone);

                        if (bone != null && bone.parent != null) {
                            return bone;
                        }
                    }
                }

                // 既存骨リストに追加されている場合
                if (currentBones.TryGetValue(targetBone.name, out var currentBone)) {
                    return currentBone;
                }

                // 今回追加した骨の場合
                if (addableBones.Contains(targetBone)) {
                    return targetBone;
                }

                return null;
            }

            foreach (var renderer in renderers) {
                // メッシュのデフォーマーになっているTransform
                targetBones.Clear();

                foreach (var targetBone in renderer.bones) {
                    if (targetBone == null) {
                        continue;
                    }

                    var bone = GetCurrentBone(targetBone);

                    // 差し替え対象の骨リストとして列挙し直す
                    if (bone != null) {
                        targetBones.Add(bone);
                    }
                }

                // 骨の参照を差し替える
                renderer.bones = targetBones.ToArray();

                // Root骨を差し替える
                var rootBone = GetCurrentBone(renderer.rootBone);

                if (rootBone != null) {
                    renderer.rootBone = rootBone;
                }
                else {
                    renderer.rootBone = boneController.Root;
                }
            }

            // 不要なTransformを削除する
            foreach (var deleteBone in deletableBones) {
                UnityEngine.Object.DestroyImmediate(deleteBone.gameObject);
            }

            return addableBones.Concat(addedBones).ToArray();
        }

        /// <summary>
        /// 骨の削除
        /// </summary>
        private void DeleteBones(MergedInfo mergedInfo) {
            var boneController = Body.GetController<BoneController>();

            if (boneController == null || boneController.Root == null) {
                return;
            }

            // 追加した骨を削除
            foreach (var additiveBone in mergedInfo.additiveBones) {
                // 参照カウンタが無くなったら削除する
                _boneReferenceCounts.TryGetValue(additiveBone, out var referenceCount);

                if (referenceCount <= 1) {
                    _boneReferenceCounts.Remove(additiveBone);
                    UnityEngine.Object.DestroyImmediate(additiveBone.gameObject);
                }
                else {
                    _boneReferenceCounts[additiveBone] = referenceCount - 1;
                }
            }

            mergedInfo.additiveBones = Array.Empty<Transform>();
        }

        /// <summary>
        /// ユニークな骨名に変換する
        /// </summary>
        private void ConvertUniqueBoneName(Transform[] bones, string prefix) {
            foreach (var target in bones) {
                target.name = $"{prefix}-{target.name}";
            }
        }

        /// <summary>
        /// 骨のオリジナル名を取得
        /// </summary>
        public string GetOriginalBoneName(Transform bone) {
            if (bone == null) {
                return "";
            }

            var boneName = bone.name;
            var names = boneName.Split('-');
            return string.Join("-", names, Mathf.Min(1, names.Length - 1), names.Length - 1);
        }
    }
}