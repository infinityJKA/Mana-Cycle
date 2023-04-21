using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ConvoSystem {
    public class ConvoSpeaker : MonoBehaviour {
        /** Battler being displayed as speaker */
        [SerializeField] private Battle.Battler speaker;
        /** Image sprite of speaker */
        [SerializeField] private Image portrait;
        /** Gameobject for speaker's name */
        [SerializeField] private GameObject nameObj;
        /** Text GUI for speaker's name */
        [SerializeField] private TMPro.TextMeshProUGUI nameGUI;
        /** Canvas group for the speaker name box */
        [SerializeField] private CanvasGroup nameGroup;

        private static float fadeTime = 0.4f;
        private static float animDistance = 200f;
        public bool animating = false;

        public void SetSpeaker(Battle.Battler speaker, bool focused) {
            this.speaker = speaker;

            if (speaker != null) {
                gameObject.SetActive(true);
                nameObj.SetActive(true);
                portrait.sprite = speaker.sprite;
                portrait.color = new Color(1.0f, 1.0f, 1.0f, focused ? 1.0f : 0.5f);
                nameGUI.text = speaker.name;
            } 
            
            else {
                gameObject.SetActive(false);
                nameObj.SetActive(false);
            }
        }

        public void StartAnim(ConvoAnim anim) {
            if (anim == ConvoAnim.None) return;
            StartCoroutine(Animate(anim));
        }

        IEnumerator Animate(ConvoAnim anim) {
            Vector2 startOffset = Vector2.zero;
            Vector2 targetOffset = Vector2.zero;
            Color startColor = Color.white;
            Color targetColor = Color.white;

            switch (anim) {
                case ConvoAnim.In:
                case ConvoAnim.InLeft:
                case ConvoAnim.InRight:
                case ConvoAnim.InUp:
                case ConvoAnim.InDown:
                    startColor = Color.clear; targetColor = Color.white; targetOffset = Vector2.zero; break;
                case ConvoAnim.Out:
                case ConvoAnim.OutLeft:
                case ConvoAnim.OutRight:
                case ConvoAnim.OutUp:
                case ConvoAnim.OutDown:
                    startColor = Color.white; targetColor = Color.clear; startOffset = Vector2.zero; break;
            }

            switch (anim) {
                case ConvoAnim.InLeft: startOffset = Vector2.left; break;
                case ConvoAnim.InRight: startOffset = Vector2.right; break;
                case ConvoAnim.InUp: startOffset = Vector2.up; break;
                case ConvoAnim.InDown: startOffset = Vector2.down; break;
                case ConvoAnim.OutLeft: targetOffset = Vector2.left; break;
                case ConvoAnim.OutRight: targetOffset = Vector2.right; break;
                case ConvoAnim.OutUp: targetOffset = Vector2.up; break;
                case ConvoAnim.OutDown: targetOffset = Vector2.down; break;
            }

            float t = 0;
            animating = true;

            while (animating && t < fadeTime) {
                t += Time.unscaledDeltaTime;

                portrait.rectTransform.anchoredPosition = Vector2.Lerp(startOffset, targetOffset, t/fadeTime) * animDistance;
                portrait.color = Color.Lerp(startColor, targetColor, t/fadeTime);
                nameGroup.alpha = Mathf.Lerp(startColor.a, targetColor.a, t/fadeTime);

                yield return null;
            }

            portrait.rectTransform.anchoredPosition = Vector2.zero;
            portrait.color = targetColor;
            animating = false;
        }
    }
}