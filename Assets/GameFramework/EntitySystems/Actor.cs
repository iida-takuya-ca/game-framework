using System;
using GameFramework.BodySystems;
using GameFramework.LogicSystems;

namespace GameFramework.EntitySystems {
    /// <summary>
    /// Body制御用ロジックのInterface
    /// </summary>
    public interface IActor : IDisposable {
        // 制御優先度
        int Priority { get; }
    }
    
    /// <summary>
    /// Body制御用ロジック
    /// </summary>
    public abstract class Actor : Logic, IActor {
        private int _priority;
        
        // 制御対象のBody
        public Body Body { get; private set; }
        
        // 制御優先度
        int IActor.Priority => _priority;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="body">制御対象のBody</param>
        /// <param name="priority">複数のActorがBodyを制御した場合の優先度</param>
        public Actor(Body body, int priority) {
            Body = body;
            _priority = priority;
        }
    }
}