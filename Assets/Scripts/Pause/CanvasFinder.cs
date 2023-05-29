#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

// lil debug tool because i fricked up some scenes by adding canvases everywhere and graphic raycasting doesn't work right

namespace Pause {
    public class CanvasFinder : MonoBehaviour
    {

    }

    [CustomEditor(typeof(CanvasFinder))]
    public class LevelListerEditor : Editor {
        

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Find canvas")) {
                foreach (Canvas canvas in GameObject.FindObjectsOfType<Canvas>()) {
                    Debug.Log(canvas);

                    Transform obj = canvas.transform;

                    while (obj) {
                        obj = obj.transform.parent;
                        if (obj) Debug.Log(obj.gameObject);
                    }

                    Debug.Log("---");
                }
            }
        }
    }
}

#endif