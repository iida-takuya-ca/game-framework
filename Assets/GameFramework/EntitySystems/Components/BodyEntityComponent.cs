using GameFramework.BodySystems;
using GameFramework.Core;

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
        /// <param name="prevDispose">既に設定されているBodyをDisposeするか</param>
        public Entity SetBody(Body body, bool prevDispose = true) {
            if (prevDispose) {
                Body?.Dispose();
            }
            Body = body;
            if (Body != null) {
                Body.IsActive = Entity.IsActive;
            }

            return Entity;
        }
        
        /// <summary>
        /// Bodyの削除
        /// </summary>
        /// <param name="dispose">BodyをDisposeするか</param>
        public Entity RemoveBody(bool dispose = true) {
            return SetBody(null, dispose);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            if (Body != null) {
                Body.IsActive = true;
            }
        }
        
        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            if (Body != null) {
                Body.IsActive = false;
            }
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            RemoveBody();
        }
    }
}