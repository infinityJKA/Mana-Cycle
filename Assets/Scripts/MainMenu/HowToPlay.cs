using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace MainMenu {
    /** Controls the HowToPlay menu and updates its pages/content. */
    public class HowToPlay : MonoBehaviour
    {
        [SerializeField] private string[] HTPTexts;
        [SerializeField] private string[] HTPTexts_jp;

        [SerializeField] private Sprite[] HTPImgs;
        private int HTPPage = 0;
        [SerializeField] private TMPro.TextMeshProUGUI currentText;
        [SerializeField] private Image currentImg;
        [SerializeField] private TMPro.TextMeshProUGUI pageText;

        [SerializeField] private InputScript inputScript;
        [SerializeField] private UnityEngine.UI.Button closeButton;

        public void Init()
        {
            HTPPage = 0;
            UpdatePage();
        }

        void Update()
        {
            if (Input.GetKeyDown(inputScript.Pause)) {
                closeButton.onClick.Invoke();
            }
        }

        public void ChangePage(int change)
        {
            HTPPage += change;
            HTPPage = Utils.mod(HTPPage, HTPTexts.Length);
            UpdatePage();
            
        }

        public void UpdatePage()
        {
            if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                currentText.text = HTPTexts_jp[HTPPage];
            }
            else{
                currentText.text = HTPTexts[HTPPage];
            }
            currentImg.sprite = HTPImgs[HTPPage];
            pageText.text = "(" + (HTPPage+1) + " / " + HTPTexts.Length + ")";
        }

    }
}