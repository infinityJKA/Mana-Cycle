using Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;

namespace Menus
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private HalfRadialButtons settingsMenu;
        [SerializeField] private GameObject[] subMenus;

        [SerializeField] private GameObject sliderSFX;
        [SerializeField] private GameObject returnSFX;
        [SerializeField] private GameObject selectionSFX;
        [SerializeField] private GameObject specialSFX;
        [SerializeField] private GameObject navigationSFX;

        private bool syncing;
        // first item to select in each submenu
        private int lastIndex = -1;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            settingsMenu.ButtonSelected += OnButtonSelected;
            foreach (GameObject o in subMenus) o.SetActive(false);

            PlayerPrefSetter[] prefSetters = Resources.FindObjectsOfTypeAll<PlayerPrefSetter>();

            syncing = true;
            foreach (PlayerPrefSetter p in prefSetters) p.Sync();
            syncing = false;
            SoundManager.Instance.UpdateMusicVolume();
        }

        void OnButtonSelected(int index, bool direction = true)
        {
            if (lastIndex >= 0) subMenus[lastIndex].SetActive(false);
            if (index < subMenus.Length) 
            {
                subMenus[index].SetActive(true);
                lastIndex = index;
            }
            Instantiate(navigationSFX).GetComponent<SFXObject>().pitch = direction ? 1.1f : 1f;
        }

        public void SetScreenMode(int mode)
        {
            var fsm = mode switch
            {
                0 => FullScreenMode.FullScreenWindow,
                1 => FullScreenMode.ExclusiveFullScreen,
                2 => FullScreenMode.Windowed,
                _ => FullScreenMode.FullScreenWindow,
            };
            Screen.fullScreenMode = fsm;
        }

        public void SetLocale(int l)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[l];
        }

        // Used to serialize in prefab where eventsystem does not exist
        public void SetSelectedGameObject(GameObject s)
        {
            EventSystem.current.SetSelectedGameObject(s);
        }

        public void PlaySliderSound(bool special)
        {
            if (!syncing)
                Instantiate(special ? specialSFX : sliderSFX);
        }

        public void PlaySelectionSound()
        {
            if (!syncing)
                Instantiate(selectionSFX);   
        }

        public void PlayReturnSound()
        {
            if (!syncing)
                Instantiate(returnSFX);
        }

        public void PlaySound(GameObject sound)
        {
            if (!syncing)
                Instantiate(sound);
        }

        public void VolumeSliderChanged()
        {
            SoundManager.Instance.UpdateMusicVolume();
        }
    }   
}

