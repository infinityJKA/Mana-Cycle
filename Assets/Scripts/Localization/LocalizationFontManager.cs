using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Localizaton {
    public class LocalizationFontManager : MonoBehaviour {
        public static LocalizationFontManager instance {get; private set;} = null;

        [SerializeField] private TMP_FontAsset enSansOutline, jaSansOutline;
        [SerializeField] private TMP_FontAsset enSans, jaSans;
        [SerializeField] private TMP_FontAsset enPixel, jaPixel;

        [SerializeField] private Material enSansOutlineMaterial, jaSansOutlineMaterial;
        [SerializeField] private Material enSansMaterial, jaSansMaterial;
        [SerializeField] private Material enPixelMaterial, jaPixelMaterial;

        public TMP_FontAsset sansOutline {get; private set;}
        public TMP_FontAsset sans {get; private set;}
        public TMP_FontAsset pixel {get; private set;}

        public Material sansOutlineMaterial {get; private set;}
        public Material sansMaterial {get; private set;}
        public Material pixelMaterial {get; private set;}

        public UnityEvent onLanguageChanged {get; set;}

        public enum FontType {
            SansOutline,
            Sans,
            Pixel
        }

        private void Awake() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }
            
            instance = this;

            DontDestroyOnLoad(gameObject);

            onLanguageChanged = new UnityEvent();

            LocalizationSettings.Instance.OnSelectedLocaleChanged += UpdateFonts;

            UpdateFonts(LocalizationSettings.SelectedLocale);
        }


        private void AddLocalizedFontComponentsToStringEvents() {
            foreach (var stringEvent in FindObjectsOfType<LocalizeStringEvent>()) {
                if (stringEvent.GetComponent<LocalizedFontText>() == null) {
                    stringEvent.gameObject.AddComponent<LocalizedFontText>();
                }
            }
        }

        private void UpdateFonts(Locale locale) {
            switch (locale.Identifier.Code) {
                case "en":
                case "de":
                case "fr":
                    sansOutline = enSansOutline;
                    sansOutlineMaterial = enSansOutlineMaterial;
                    sans = enSans;
                    sansMaterial = enSansMaterial;
                    pixel = enPixel;
                    pixelMaterial = enPixelMaterial;
                    break;
                case "ja":
                    sansOutline = jaSansOutline;
                    sansOutlineMaterial = jaSansOutlineMaterial;
                    sans = jaSans;
                    sansMaterial = jaSansMaterial;
                    pixel = jaPixel;
                    pixelMaterial = jaPixelMaterial;
                    break;
                default:
                    Debug.LogWarning("Mana Cycle does not recognize fonts for locale "+LocalizationSettings.SelectedLocale.Identifier.Code+"!");
                    return;
            }
            Debug.Log("locale set to "+locale.Identifier.Code);

            onLanguageChanged.Invoke();
        }
    }
}