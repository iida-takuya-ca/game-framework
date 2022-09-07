using GameFramework.BodySystems;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// BodyをEntityと紐づけるためのComponent
    /// </summary>
    public class BodyEntityComponent : EntityComponent {
        // 現在のBody
        public Body Body { get; private set; } = null;
        
        /// <summary>
        /// Bodyの設定
        /// </summary>
        /// <param name="body">設定するBody</param>
        public void SetBody(Body body) {
            Body?.Dispose();
            Body = body;
        }
        
        /// <summary>
        /// Bodyの削除
        /// </summary>
        public void RemoveBody() {
            SetBody(null);
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            RemoveBody();
        }
    }
}