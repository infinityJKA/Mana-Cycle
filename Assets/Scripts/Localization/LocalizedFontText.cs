using TMPro;
using UnityEngine;

namespace Localizaton {
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedFontText : MonoBehaviour {
        private TMP_Text text;

        [SerializeField] private LocalizationFontManager.FontType fontType;

        private void Start() {
            text = GetComponent<TMP_Text>();
            UpdateFont();
            LocalizationFontManager.instance.onLanguageChanged.AddListener(UpdateFont);
        }

        private void UpdateFont() {
            switch (fontType) {
                case LocalizationFontManager.FontType.SansOutline:
                    text.font = LocalizationFontManager.instance.sansOutline;
                    text.fontSharedMaterial = LocalizationFontManager.instance.sansOutlineMaterial;
                    break;
            }
            Debug.Log("font updated on "+gameObject);
        }

        // also called on destroy / when switching scenes
        private void OnDisable() {
            LocalizationFontManager.instance.onLanguageChanged.RemoveListener(UpdateFont);
        }
    }
}