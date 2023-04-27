using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

namespace VersusMode {
    ///<summary>Controls a character icon in the character select grid. </summary>

    public class CharacterIcon : MonoBehaviour {
        //<summary>Currently displayed battler.</summary>
        [SerializeField] private Battle.Battler battler;

        //<summary>Icon background where gradient material is set</summary>
        [SerializeField] private Image background;
        //<summary>Image for the battler portrait</summary>
        [SerializeField] private Image portrait;
        //<summary>Images for the players & cpu cursor overlayed on this icon.</summary>
        [SerializeField] private UnityEngine.UI.Image cursorImage, cpuCursorImage;

        // serialized refs to all the different cursor sprites
        [SerializeField] private Sprite p1CursorSprite, p2CursorSprite, bothCursorSprite, cpuCursorSprite;
        
        ///<summary>Whether or not p1/p2 is currently hovered over.</summary>
        private bool p1hovered, p2hovered, cpuHovered;

        ///<summary>Set Whether or not p1/p2 is currently hovered over.</summary>
        ///<param name="isP1">true for p1, false for p2</param>
        ///<param name="hovered">true if the player's cursor is here, false if not</param>
        public void SetHovered(bool isP1, bool hovered) {
            if (isP1) {
                p1hovered = hovered;
            } else {
                p2hovered = hovered;
            }
            RefreshCursorImage();
        }

        ///<summary>Same as SetHovered but only for the cup cursor</summary>
        public void SetCUPHovered(bool hovered) {
            cpuHovered = hovered;
            RefreshCursorImage();
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
            background.material = battler.gradientMat;
            portrait.sprite = battler.sprite;
        }
    }
}