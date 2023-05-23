namespace SampleGame.ModelViewer {
    /// <summary>
    /// ModelViewerBodyDataのAssetRequest
    /// </summary>
    public class ModelViewerBodyDataRequest : DataAssetRequest<ModelViewerBodyData> {
        public override string Address { get; }
        
        public ModelViewerBodyDataRequest(string assetKey) {
            Address = GetPath($"ModelViewer/Body/dat_model_viewer_body_{assetKey}.asset");
        }
    }
}