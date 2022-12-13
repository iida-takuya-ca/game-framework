using System;
using GameFramework.Core;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SampleGame {
    /// <summary>
    /// Sample用のAssetRequest基底
    /// </summary>
    public abstract class GameAssetRequest<T> : AssetRequest
        where T : Object {
        public GameAssetRequest(string path)
            : base(path) {
        }

        /// <summary>
        /// アセットの読み込み処理
        /// </summary>
        public IObservable<T> LoadAsync() {
            return Observable.Defer(() => {
                // 取り合えずサンプルなのでUnityEditorで読み込み
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);
                return Observable.Return(asset);
            });
            // return Resources.LoadAsync<T>(Path)
            //     .AsAsyncOperationObservable()
            //     .Select(op => (T)op.asset);
        }
    }

    /// <summary>
    /// BodyPrefabのAssetRequest基底
    /// </summary>
    public abstract class BodyPrefabAssetRequest : GameAssetRequest<GameObject> {
        public BodyPrefabAssetRequest(string relativePath)
            : base($"Assets/SampleGame/BodyAssets/{relativePath}") {
        }
    }

    /// <summary>
    /// PlayerPrefabのAssetRequest
    /// </summary>
    public class PlayerPrefabAssetRequest : BodyPrefabAssetRequest {
        public PlayerPrefabAssetRequest(string assetKey)
            : base($"Player/{assetKey}/Models/prfb_{assetKey}.prefab") {
        }
    }

    /// <summary>
    /// PlayerActorSetupDataのAssetRequest
    /// </summary>
    public class PlayerActorSetupDataAssetRequest : GameAssetRequest<PlayerActorSetupData> {
        public PlayerActorSetupDataAssetRequest(string assetKey)
            : base($"Assets/SampleGame/Data/PlayerActorSetup/dat_player_actor_setup_{assetKey}.asset") {
        }
    }

    /// <summary>
    /// PlayerActorActionDataのAssetRequest
    /// </summary>
    public class PlayerActorActionDataAssetRequest : GameAssetRequest<PlayerActorActionData> {
        public PlayerActorActionDataAssetRequest(string assetKey)
            : base($"Assets/SampleGame/Data/PlayerActorAction/dat_player_actor_action_{assetKey}.asset") {
        }
    }

    /// <summary>
    /// BattlePlayerMasterDataのAssetRequest
    /// </summary>
    public class BattlePlayerMasterDataAssetRequest : GameAssetRequest<BattlePlayerMasterData> {
        public BattlePlayerMasterDataAssetRequest(string assetKey)
            : base($"Assets/SampleGame/Data/Battle/BattlePlayerMaster/dat_battle_player_master_{assetKey}.asset") {
        }
    }
}