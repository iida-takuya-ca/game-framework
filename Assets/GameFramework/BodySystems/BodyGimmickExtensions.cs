namespace GameFramework.BodySystems {
    /// <summary>
    /// BodyGimmick用の拡張メソッド
    /// </summary>
    public static class BodyGimmickExtensions {
        /// <summary>
        /// ActiveGimmickを取得
        /// </summary>
        public static ActiveGimmick[] GetActiveGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<ActiveGimmick>(key);
        }
        
        /// <summary>
        /// Activate操作
        /// </summary>
        public static void Activate(this ActiveGimmick[] source) {
            foreach (var gimmick in source) {
                gimmick.Activate();
            }
        }
        
        /// <summary>
        /// Deactivate操作
        /// </summary>
        public static void Deactivate(this ActiveGimmick[] source) {
            foreach (var gimmick in source) {
                gimmick.Deactivate();
            }
        }
        /// <summary>
        /// AnimationGimmickを取得
        /// </summary>
        public static AnimationGimmick[] GetAnimationGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<AnimationGimmick>(key);
        }
        
        /// <summary>
        /// Play操作
        /// </summary>
        public static void Play(this AnimationGimmick[] source, bool reverse = false) {
            foreach (var gimmick in source) {
                gimmick.Play(reverse);
            }
        }
        
        /// <summary>
        /// Resume操作
        /// </summary>
        public static void Resume(this AnimationGimmick[] source, bool reverse = false) {
            foreach (var gimmick in source) {
                gimmick.Resume(reverse);
            }
        }
    }
}