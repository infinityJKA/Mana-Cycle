using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleAnimator : MonoBehaviour {
    [SerializeField] private Toggle toggle;

    [SerializeField] private Animator animator;

    [SerializeField] private TMP_Text label;

    [SerializeField] private string offLabel, onLabel;

    private void Start() {
        Animate();
        toggle.onValueChanged.AddListener((newValue) => {
            Animate();
        });
    }

    private void Animate() {
        animator.SetBool("isOn", toggle.isOn);
        UpdateLabel();
    }

    public void UpdateLabel() {
        if (toggle.isOn) {
            label.text = onLabel;
        } else {
            label.text = offLabel;
        }
    }
}