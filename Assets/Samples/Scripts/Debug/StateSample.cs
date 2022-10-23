using UnityEngine;
using GameFramework.StateSystems;

/// <summary>
/// ステート制御サンプル
/// </summary>
public class StateSample : MonoBehaviour {
    private enum State {
        Invalid = -1,
        Standby,
        Vertical,
        Horizontal,
    }
    
    [SerializeField, Tooltip("縦向き用オブジェクト")]
    private GameObject[] _verticalObjects;
    [SerializeField, Tooltip("横並び用オブジェクト")]
    private GameObject[] _horizontalObjects;

    private EnumStateContainer<State> _stateContainer = new EnumStateContainer<State>();

    /// <summary>
    /// 生成時処理
    /// </summary>
    private void Awake() {
        void SetActiveObjects(GameObject[] targets, bool active) {
            foreach (var target in targets) {
                if (target == null) {
                    continue;
                }
                target.SetActive(active);
            }
        }
        
        _stateContainer.SetupFromEnum(State.Invalid);
        _stateContainer.SetFunction(State.Standby,
            (prev, scope) => {
                SetActiveObjects(_verticalObjects, false);
                SetActiveObjects(_horizontalObjects, false);
            });
        _stateContainer.SetFunction(State.Vertical,
            (prev, scope) => SetActiveObjects(_verticalObjects, true),
            _ => {},
            (next) => SetActiveObjects(_verticalObjects, false));
        _stateContainer.SetFunction(State.Horizontal,
            (prev, scope) => SetActiveObjects(_horizontalObjects, true),
            _ => {},
            (next) => SetActiveObjects(_horizontalObjects, false));
    }

    /// <summary>
    /// 開始時処理
    /// </summary>
    private void Start() {
        _stateContainer.Change(State.Standby, true);
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            _stateContainer.Change(State.Horizontal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            _stateContainer.Change(State.Vertical);
        }
        
        _stateContainer.Update(Time.deltaTime);
    }

    /// <summary>
    /// 廃棄時処理
    /// </summary>
    private void OnDestroy() {
        _stateContainer.Dispose();
    }
}