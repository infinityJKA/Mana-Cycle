using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Battler", menuName = "ManaCycle/Input Script")]
public class InputScript : ScriptableObject
{
    public InputScript defaults;

    public KeyCode Left;
    public KeyCode Right;
    public KeyCode Down;
    public KeyCode Up;
    public KeyCode RotateCCW;
    public KeyCode RotateCW;
    public KeyCode Cast;
    public KeyCode Pause;

    public void resetToDefault()
    {
        if (defaults == null) return;

        Left = defaults.Left;
        Right = defaults.Right;
        Down = defaults.Down;
        Up = defaults.Up;
        RotateCCW = defaults.RotateCCW;
        RotateCW = defaults.RotateCW;
        Cast = defaults.Cast;
        Pause = defaults.Pause;

        Debug.Log("Binds Reset");

    }

    
    #if (UNITY_EDITOR)
    [CustomEditor(typeof(InputScript))]
    public class InputScriptEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Set to default")) {
                InputScript thisScript = (InputScript) target;
                thisScript.resetToDefault();
            }
        }
    }
    #endif

}