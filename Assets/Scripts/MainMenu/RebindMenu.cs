using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.EventSystems;

namespace MainMenu {
    /// <summary>
    /// Controls input rebinding and rebinding window
    /// </summary>
    public class RebindMenu : MonoBehaviour
    {
        [SerializeField] private GameObject settingsWindow;
        [SerializeField] private TMPro.TextMeshProUGUI bindText;
        [SerializeField] private Menu3d mainMenu;
        private InputScript inputs;
        // control currently being rebinded
        private int currentIndex;

        private KeyCode[] keyList;
        
        void OnGUI()
        {

            // get the key the user is pressing and set bind
            Event e = Event.current;
            if (e.isKey && Input.GetKeyDown(e.keyCode))
            {
                Debug.Log("Detected key code: " + e.keyCode);

                // don't let user assign the same key to multiple binds
                if ((keyList[currentIndex] == e.keyCode) || (!keyList.Contains(e.keyCode)))
                {
                    // epic bind succsess 
                    keyList[currentIndex] = e.keyCode;
                    setBind(e.keyCode);
                    currentIndex++;
                }
                else
                {
                    // epic bind failure
                    Debug.Log("lol. lmao");
                }

                refreshText();
                
                // close rebind window when all keys are binded
                if (currentIndex >= keyList.Length) EndRebind(); 
            }
        }

        public void StartRebind(InputScript inputScript)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("rebind start");
            inputs = inputScript;
            keyList = new KeyCode[] {inputs.Up, inputs.Left, inputs.Down, inputs.Right, inputs.RotateLeft, inputs.RotateRight, inputs.Cast, inputs.Pause};
            // set window visibility
            gameObject.SetActive(true);
            settingsWindow.SetActive(false);

            currentIndex = 0;

            refreshText();

        }

        public void EndRebind()
        {
            gameObject.SetActive(false);
            // settingsWindow.SetActive(true);
            mainMenu.CloseSettings();
        }

        public void refreshText()
        {

            bindText.text = "";
            for (int i = 0; i < keyList.Length; i++)
            {   
                KeyCode keyCode = keyList[i];
                bindText.text += Utils.KeySymbol(keyCode) + (currentIndex == i ? " <" : "") + "\n";
            }
        }

        public void setBind(KeyCode k){
            // setting the values in keyList won't change the vars themselves
            // still probably a better way to do this
            switch(currentIndex)
            {
                case 0: inputs.Up = k; break;
                case 1: inputs.Left = k; break;
                case 2: inputs.Down = k; break;
                case 3: inputs.Right = k; break;
                case 4: inputs.RotateLeft = k; break;
                case 5: inputs.RotateRight = k; break;
                case 6: inputs.Cast = k; break;
                case 7: inputs.Pause = k; break;
                default: break;
            }
        }
    }
}
