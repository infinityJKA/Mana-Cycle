using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackPopup : MonoBehaviour
{
    /** Battler's portrait within the popup */
    [SerializeField] Image portrait;
    /** Image of the background, material matches the battler's portrait */
    [SerializeField] Image background;
    /** Canvas group that controls all alpha values in the popup */
    [SerializeField] CanvasGroup canvasGroup;

    /** Starting x offset of the popup */
    [SerializeField] float popupStartOffset;
    /** Ending x offset of the popup */
    [SerializeField] float popupEndOffset;

    /** Starting x offset of the portrait */
    [SerializeField] float portraitStartOffset;
    /** Ending x offset of the portrait */
    [SerializeField] float portraitEndOffset;

    /** Total time spent moving, not including fade time */
    [SerializeField] float moveTime = 1.25f;
    /** Total move time of the portrait */
    [SerializeField] float portraitMoveTime = 1.5f;
    /** Amount of fade delay on the start of the animation */
    [SerializeField] float startFadeTime = 0.25f;
    /** Time before starting to fade out */
    [SerializeField] float timeUntilFade = 1.25f;
    /** Amount of time spend fading out */
    [SerializeField] float endFadeTime = 0.25f;

    /** If currently animating */
    bool animating;
    /** Central reference point for the popup */
    Vector2 popupCenter;
    /** Central reference point for the portrait */
    Vector2 portraitCenter;
    /** Time that the popup display started. */
    float animStartTime;

    void Start() {
        canvasGroup.alpha = 0;
    }

    void Update() {
        if (animating) {
            var timeElapsed = Time.time - animStartTime;
        
            // control opacity
            if (timeElapsed < startFadeTime) {
                canvasGroup.alpha = Mathf.SmoothStep(0, 1, timeElapsed/startFadeTime);
            } else if (timeElapsed < timeUntilFade) {
                canvasGroup.alpha = 1;
            } else if (timeElapsed < timeUntilFade+endFadeTime) {
                canvasGroup.alpha = Mathf.SmoothStep(1, 0, (timeElapsed-timeUntilFade)/endFadeTime);
            } else {
                animating = false;
                transform.localPosition = popupCenter;
                portrait.transform.localPosition = portraitCenter;
                return;
            }

            // control movement of popup and portrait
            transform.localPosition = popupCenter + Vector2.right * Mathf.SmoothStep(popupStartOffset, popupEndOffset, timeElapsed/moveTime);
            portrait.transform.localPosition = portraitCenter + Vector2.right * Mathf.SmoothStep(portraitStartOffset, portraitEndOffset, timeElapsed/portraitMoveTime);
        }
    }

    // Shows this popup for a brief moment with the given value.
    public void AttackAnimation() {
        // only reset start position if not animating
        if (!animating) {
            popupCenter = transform.localPosition;
            portraitCenter = portrait.transform.localPosition;
        }
        animStartTime = Time.time;
        animating = true;
    }

    // Set to the board's battler, called from board on game start
    public void SetBattler(Battler battler) {
        portrait.sprite = battler.sprite;
        background.material = battler.gradientMat;
    }
}
