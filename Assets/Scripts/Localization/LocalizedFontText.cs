using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;


[RequireComponent(typeof(TMP_Text))]
public class LocalizedFontText : MonoBehaviour {
    private TMP_Text text;

    [SerializeField] private LocalizationFontManager.FontType fontType;

    [SerializeField] private Material enMaterialOverride, jaMaterialOverride;
    [Tooltip("0 for no change")]
    [SerializeField] private float enFontSize, jaFontSize;

    private void OnEnable() {
        UpdateFont();
        LocalizationFontManager.instance.onLanguageChanged.AddListener(UpdateFont);
    }

    private void UpdateFont() {
        text = GetComponent<TMP_Text>();

        switch (fontType) {
            case LocalizationFontManager.FontType.Sans:
                text.font = LocalizationFontManager.instance.sans;
                text.fontSharedMaterial = LocalizationFontManager.instance.sansMaterial;
                break;
            case LocalizationFontManager.FontType.SansOutline:
                text.font = LocalizationFontManager.instance.sansOutline;
                text.fontSharedMaterial = LocalizationFontManager.instance.sansOutlineMaterial;
                break;
            case LocalizationFontManager.FontType.SansDropShadow:
                text.font = LocalizationFontManager.instance.sansDropShadow;
                text.fontSharedMaterial = LocalizationFontManager.instance.sansDropShadowMaterial;
                break;
            case LocalizationFontManager.FontType.Pixel:
                text.font = LocalizationFontManager.instance.pixel;
                text.fontSharedMaterial = LocalizationFontManager.instance.pixelMaterial;
                break;
        }

        string localeCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        if (localeCode == "en") {
            if (enMaterialOverride) text.fontSharedMaterial = enMaterialOverride;
            if (enFontSize > 0) text.fontSize = enFontSize;
        }
        else if (localeCode == "ja") {
            if (jaMaterialOverride) text.fontSharedMaterial = jaMaterialOverride;
            if (jaFontSize > 0) text.fontSize = jaFontSize;
        }
    }

    
    // also called on destroy / when switching scenes
    private void OnDisable() {
        LocalizationFontManager.instance.onLanguageChanged.RemoveListener(UpdateFont);
    }
}