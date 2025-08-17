using UnityEngine;
using UnityEngine.EventSystems;

namespace Menus
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private HalfRadialButtons settingsMenu;
        [SerializeField] private GameObject[] subMenus;

        [SerializeField] private AudioClip sliderSFX;
        [SerializeField] private AudioClip returnSFX;
        [SerializeField] private AudioClip selectionSFX;
        [SerializeField] private AudioClip specialSFX;
        // first item to select in each submenu
        private int lastIndex = -1;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            settingsMenu.ButtonSelected += OnButtonSelected;
            foreach (GameObject o in subMenus) o.SetActive(false);

            PlayerPrefSetter[] prefSetters = Resources.FindObjectsOfTypeAll<PlayerPrefSetter>();
            foreach (PlayerPrefSetter p in prefSetters) p.Sync();
        }

        void OnButtonSelected(int index, bool direction = true)
        {
            if (lastIndex >= 0) subMenus[lastIndex].SetActive(false);
            if (index < subMenus.Length) 
            {
                subMenus[index].gameObject.SetActive(true);
                lastIndex = index;
            }
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

        // Used to serialize in prefab where eventsystem does not exist
        public void SetSelectedGameObject(GameObject s)
        {
            EventSystem.current.SetSelectedGameObject(s);
        }

        public void PlaySliderSound(bool special)
        {
            // AudioManager.Instance.PlaySound(special ? specialSFX : sliderSFX);
        }

        public void PlaySelectionSound()
        {
            // AudioManager.Instance.PlaySound(selectionSFX);   
        }

        public void PlayReturnSound()
        {
            // AudioManager.Instance.PlaySound(returnSFX);
        }

        public void VolumeSliderChanged()
        {
            // AudioManager.Instance.UpdateVolumes();
        }
    }   
}

