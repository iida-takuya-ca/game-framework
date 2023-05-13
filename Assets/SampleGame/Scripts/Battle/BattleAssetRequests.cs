namespace SampleGame.Battle {
    /// <summary>
    /// BattlePlayerMasterDataのAssetRequest
    /// </summary>
    public class BattlePlayerMasterDataAssetRequest : DataAssetRequest<BattlePlayerMasterData> {
        public override string Address { get; }
        
        public BattlePlayerMasterDataAssetRequest(string assetKey) {
            Address = GetPath($"Battle/BattlePlayerMaster/dat_battle_player_master_{assetKey}.asset");
        }
    }
}