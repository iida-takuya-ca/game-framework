namespace SampleGame.ModelViewer {
    /// <summary>
    /// Preview用のActorSetupData読み込みリクエスト
    /// </summary>
    public class PreviewActorSetupDataRequest : ActorAssetRequest<PreviewActorSetupData> {
        public override string Address { get; }
        
        public PreviewActorSetupDataRequest(string assetKey) {
            Address = GetPath($"Preview/dat_preview_actor_setup_{assetKey}.asset");
        }
    }
}