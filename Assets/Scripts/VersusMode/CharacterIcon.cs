using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

using Battle;

namespace VersusMode {
    ///<summary>Controls a character icon in the character select grid. </summary>

    public class CharacterIcon : MonoBehaviour {
        //<summary>Currently displayed battler.</summary>
        [SerializeField] private Battler _battler;
        public Battler battler {get {return _battler;}}

        //<summary>Icon background where gradient material is set</summary>
        [SerializeField] private Image background;
        //<summary>Image for the battler portrait</summary>
        [SerializeField] private Image portrait;
        //<summary>Images for the players & cpu cursor overlayed on this icon.</summary>
        [SerializeField] public UnityEngine.UI.Image cursorImage, cpuCursorImage;

        // serialized refs to all the different cursor sprites
        [SerializeField] private Sprite p1CursorSprite, p2CursorSprite, bothCursorSprite, cpuCursorSprite;
        
        ///<summary>Whether or not p1/p2 is currently hovered over.</summary>
        private bool p1hovered, p2hovered, cpuHovered;

        ///<summary>Selectable, used to find adjacent icons to select</summary>
        public Selectable selectable {get; private set;}

        ///<summary>Set Whether or not p1/p2 is currently hovered over.</summary>
        ///<param name="isPlayer1">true for p1, false for p2</param>
        ///<param name="hovered">true if the player's cursor is here, false if not</param>
        public void SetSelected(bool isPlayer1, bool hovered, bool dim = false) {
            if (isPlayer1) {
                p1hovered = hovered;
            } else if (Storage.gamemode != Storage.GameMode.Solo) {
                p2hovered = hovered;
            }
            RefreshCursorImage();
            cursorImage.color = dim ? new Color(1, 1, 1, 0.5f) : Color.white;
        }

        ///<summary>Same as SetHovered but only for the cpu cursor</summary>
        public void SetCPUHovered(bool hovered, bool dim = false) {
            cpuHovered = hovered;
            RefreshCursorImage();
            cpuCursorImage.color = dim ? new Color(1, 1, 1, 0.5f) : Color.white;
        }

        ///<summary>Refreshes the images displayed on the cursor to reflect the current cursor state</summary>
        private void RefreshCursorImage() {
            cursorImage.gameObject.SetActive(p1hovered || p2hovered);

            if (p1hovered && p2hovered) {
                cursorImage.sprite = bothCursorSprite;
            } else if (p1hovered) {
                cursorImage.sprite = p1CursorSprite;
            } else if (p2hovered) {
                cursorImage.sprite = p2CursorSprite;
            }

            cpuCursorImage.gameObject.SetActive(cpuHovered);
        }

        ///<summary>updates images and gradients in editor</summary>
        private void OnValidate() {
            if (_battler != null) {
                background.material = _battler.gradientMat;
                portrait.sprite = _battler.sprite;
                selectable = GetComponent<Selectable>();
            } else {
                Debug.LogError("Battler is null on "+gameObject.name+"!");
                Debug.Log(_battler);
            }
        }
    }
}