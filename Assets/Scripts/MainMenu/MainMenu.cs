using UnityEngine;
using UnityEngine.UI;
using Menus;
using LootLocker.Extension.DataTypes;
using Sound;

// ties multiple parts of the main menu together
namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {
            // should correspond to the color of each button
            [SerializeField] private Color[] menuColors;
            [SerializeField] private HalfRadialButtons rootMenu;
            [SerializeField] private HalfRadialButtons settingsMenu;
            [SerializeField] private HalfRadialButtons versusMenu;
            [SerializeField] private Image backgroundImage;
            [SerializeField] private ImageColorSmoother backgroundFader;
            [SerializeField] private RectTransformSmoother logoImage;

            [SerializeField] private TransitionScript TransitionHandler;

            [SerializeField] private GameObject selectSFX, submitSFX, backSFX;

            // Start is called before the first frame update
            void Start()
            {
                SoundManager.Instance.UpdateMusicVolume();
                // AudioManager.Instance.PlayMusic(menuMusic);

                rootMenu.ButtonSelected += OnRootButtonSelected;

                rootMenu.MenuOpened += RootMenuOpened;
                settingsMenu.MenuOpened += SettingsMenuOpened;

                rootMenu.ButtonSelected += PlaySelectSFX;
                // settingsMenu.ButtonSelected += PlaySelectSFX;
                settingsMenu.MenuOpened += PlaySubmitSFX;
                settingsMenu.MenuClosed += PlayBackSFX;

                settingsMenu.gameObject.SetActive(false);
                versusMenu.gameObject.SetActive(false);

                rootMenu.CoroutineOpen();
            }

            // called when a new button in the radial menu is selected. Update visuals accordingly
            private void OnRootButtonSelected(int index, bool direction = true)
            {
                // backgroundImage.materialForRendering.SetColor("_Color", Color.Lerp(menuColors[index], Color.black, 0.65f));
            }

            private void RootMenuOpened()
            {
                backgroundFader.SetAlphaTarget(0f);
                logoImage.SetTargets(new Vector2(-10, 10));
            }

            private void SettingsMenuOpened()
            {
                backgroundFader.SetAlphaTarget(0.85f);
                logoImage.SetTargets(new Vector2(750, 10));
            }

            private void PlaySelectSFX(int index, bool direction = true)
            {
                Instantiate(selectSFX).GetComponent<SFXObject>().pitch = direction ? 1.1f : 1f;
            }

            private void PlaySubmitSFX()
            {
                Instantiate(submitSFX);
            }

            private void PlayBackSFX()
            {
                Instantiate(backSFX);
            }

            public void SingleplayerPressed() 
            {
                if (BlackjackMenu.blackjackOpened) return;
                Storage.online = false;
                TransitionHandler.WipeToScene("SoloMenu");
            }

            public void LocalMultiplayerPressed(int versusType) 
            {
                // this is currently always called from main menu versus setup, so set gamemode to versus
                Storage.gamemode = Storage.GameMode.Versus;
                Storage.level = null;
                switch(versusType)
                {
                    case 0: Storage.online = false; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true; break;
                    case 1: Storage.online = false; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = false; break;
                    case 2: Storage.online = false; Storage.isPlayerControlled1 = false; Storage.isPlayerControlled2 = false; break;
                    default: Storage.online = false; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true; break;
                }
                TransitionHandler.WipeToScene("CharSelect");
            }

            public void QuitPressed()
            {
                Debug.Log("Quiting");
                Application.Quit();
            }
    }
}
