using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Localizaton {
    public class LocalizationFontManager : MonoBehaviour {
        public static LocalizationFontManager instance {get; private set;} = null;

        [SerializeField] private TMP_FontAsset enSansOutline, jaSansOutline;

        [SerializeField] private Material enSansOutlineMaterial, jsSansOutlineMaterial;

        public TMP_FontAsset sansOutline {get; private set;}

        public Material sansOutlineMaterial {get; private set;}

        public UnityEvent onLanguageChanged {get; set;}

        public enum FontType {
            SansOutline,
            Sans,
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

        private void UpdateFonts(Locale locale) {
            switch (locale.Identifier.Code) {
                case "en":
                    sansOutline = enSansOutline;
                    sansOutlineMaterial = enSansOutlineMaterial;
                    break;
                case "ja":
                    sansOutline = jaSansOutline;
                    sansOutlineMaterial = jsSansOutlineMaterial;
                    break;
                default:
                    Debug.LogWarning("Mana CYcle does not recognize fonts for locale "+LocalizationSettings.SelectedLocale.Identifier.Code+"!");
                    return;
            }
            Debug.Log("locale set to "+locale.Identifier.Code);

            onLanguageChanged.Invoke();
        }
    }
}