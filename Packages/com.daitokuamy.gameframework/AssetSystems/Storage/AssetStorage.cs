using System;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセット格納クラス
    /// </summary>
    public abstract class AssetStorage<T> : IAssetStorage
        where T : AssetStorage<T>, new() {
        private static T s_storage;

        // アセット管理クラス
        protected AssetManager AssetManager { get; private set; }
        
        /// <summary>
        /// ストレージの生成
        /// </summary>
        public static T Create(AssetManager assetManager) {
            if (s_storage == null) {
                s_storage = new T();
                ((IAssetStorage)s_storage).Initialize(assetManager);
            }
            
            return s_storage;
        }

        /// <summary>
        /// ストレージの取得
        /// </summary>
        public static T Get() {
            return s_storage;
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        public static void Delete() {
            if (s_storage == null) {
                return;
            }
            
            ((IDisposable)s_storage).Dispose();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IAssetStorage.Initialize(AssetManager assetManager) {
            s_storage.AssetManager = assetManager;
            InitializeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            DisposeInternal();
            
            if (this == s_storage) {
                s_storage = null;
            }
        }

        /// <summary>
        /// 初期化処理(Override用)
        /// </summary>
        protected virtual void InitializeInternal() {
        }

        /// <summary>
        /// 廃棄時処理(Override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }
    }
}