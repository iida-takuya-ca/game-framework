using System;
using GameFramework.Core;
using UniRx;
using UnityEngine.SceneManagement;

namespace SampleGame {
    /// <summary>
    /// シーン用のユーティリティ
    /// </summary>
    public static class SceneUtility {
        /// <summary>
        /// 加算シーンの読み込み処理（配置含む）
        /// </summary>
        public static IObservable<Scene> LoadAdditiveSceneAsync(string relativeScenePath, IScope scope) {
            return Observable.Create<Scene>(observer => {
                var request = new SceneAssetRequest(relativeScenePath, LoadSceneMode.Additive);
                return request.LoadAsync(scope)
                    .DoOnError(observer.OnError)
                    .SelectMany(instance => {
                        var op = instance.ActivateAsync();
                        return op.AsAsyncOperationObservable()
                            .Do(_ => {
                                var scene = instance.Scene;
                                scope.OnExpired += () => SceneManager.UnloadSceneAsync(scene);
                                observer.OnNext(scene);
                                observer.OnCompleted();
                            });
                    })
                    .Subscribe();
            });
        }
    }
}