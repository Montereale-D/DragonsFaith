using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuManager : Singleton<MenuManager>
    {
        private enum Menu
        {
            Main,
            Play,
            Options,
            Audio,
            Graphics,
            Keybindings,
            Credits
        };
    
        public Animator animator;
        private static readonly int Quit = Animator.StringToHash("Quit");
    
        [Header("Menus")]
        public GameObject mainMenu;
        public GameObject playMenu;
        public GameObject optionsMenu;
        public GameObject audioMenu;
        public GameObject graphicsMenu;
        public GameObject keybindingsMenu;
        public GameObject creditsMenu;

        [Header("Play Screens")] 
        public GameObject playAs;
        public GameObject hostOrClient;
        public GameObject host;
        public GameObject client;
        
        [Header("Option Sliders")]
        public Slider playerVolumeSlider;
        public Slider enemyVolumeSlider;
        public Slider backgroundVolumeSlider;

        [Header("Key Bind Texts")] 
        public Text move;
        public Text dash;
        public Text interact;
        public Text potion; 
        public Text spell;
        
        [Header("Pop Up")]
        public PopUpMessage toolTip;

        [Header("Resolutions")]
        private Resolution[] _resolutions;
        public Dropdown resolutionDropdown;

        private GameObject[] _keybindingButtons;
        public static bool isChangingKey;
    
        //private UnityEngine.InputSystem.PlayerInput _playerInput;


        //generic function to activate a certain menu screen
        private void SetMenu(Menu menu)
        {
            mainMenu.SetActive(false);
            playMenu.SetActive(false);
            optionsMenu.SetActive(false);
            audioMenu.SetActive(false);
            graphicsMenu.SetActive(false);
            keybindingsMenu.SetActive(false);
            creditsMenu.SetActive(false);

            switch (menu)
            {
                case Menu.Main:
                    mainMenu.SetActive(true);
                    break;
                case Menu.Play:
                    playMenu.SetActive(true);
                    playAs.SetActive(true);
                    hostOrClient.SetActive(false);
                    break;
                case Menu.Options:
                    optionsMenu.SetActive(true);
                    break;
                case Menu.Audio:
                    audioMenu.SetActive(true);
                    break;
                case Menu.Graphics:
                    graphicsMenu.SetActive(true);
                    break;
                case Menu.Keybindings:
                    keybindingsMenu.SetActive(true);
                    break;
                case Menu.Credits:
                    creditsMenu.SetActive(true);
                    break;
            }
        }

        private void Start()
        {
            SetMenu(Menu.Main);

            /*_playerInput = //InputManager.Instance.playerInput;
            _playerInput.camera = Camera.main;
            _playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();*/
        
            //_playerInput.SwitchCurrentActionMap("UI");
        
            var resolutions = Screen.resolutions.Select(resolution => new Resolution
            {
                width = resolution.width, height = resolution.height
            }).Distinct();
            _resolutions = resolutions as Resolution[] ?? resolutions.ToArray();
        
            resolutionDropdown.ClearOptions();
            var options = new List<string>();
            var currentResolutionIndex = 0;
            for (var i = 0; i < _resolutions.Length; i++)
            {
                var option = _resolutions[i].width + "x" + _resolutions[i].height;
                options.Add(option);
    
                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        
            /*playerVolumeSlider.value = AudioManager.Instance.GetPlayerVolumeSound();
            enemyVolumeSlider.value = AudioManager.Instance.GetEnemyVolumeSound();
            backgroundVolumeSlider.value = AudioManager.Instance.GetBackgroundVolumeSound();*/
        }

        private void Update()
        {
            if ((audioMenu.activeSelf ||
                 graphicsMenu.activeSelf ||
                 (keybindingsMenu.activeSelf /*&& !isChangingKey*/)) && Input.GetKeyDown(KeyCode.Escape)
                /*_playerInput.actions["Back"].WasPressedThisFrame()*/)   //return to main menu
            {
                SetMenu(Menu.Options);
                //InputManager.Instance.ResetIfNotSaved(_playerInput);
            }
            /*else if (keybindingsMenu.activeSelf && isChangingKey)
            {
            }*/
            else if (Input.GetKeyDown(KeyCode.Escape)/*_playerInput.actions["Back"].WasPressedThisFrame()*/)
            {
                SetMenu(Menu.Main);
            }
        }
        
        public void UpdateText(UnityEngine.InputSystem.PlayerInput player)
        {
            move.text = player.actions["Move"].bindings[1].ToDisplayString() + "/" +
                        player.actions["Move"].bindings[2].ToDisplayString() + "/" +
                        player.actions["Move"].bindings[3].ToDisplayString() + "/" +
                        player.actions["Move"].bindings[4].ToDisplayString();
            dash.text = player.actions["Dash"].bindings[0].ToDisplayString();
            interact.text = player.actions["Interact"].bindings[0].ToDisplayString();
            potion.text = player.actions["Potion"].bindings[0].ToDisplayString();
            spell.text = player.actions["Spell"].bindings[0].ToDisplayString();
        }

        public void OpenNotification()
        { 
            toolTip.StartOpen();
        }
    
        //reactors to the pressing of a button
        public void OpenMainMenu()
        {
            SetMenu(Menu.Main);
        }
        
        public void OpenPlayMenu()
        {
            SetMenu(Menu.Play);
        }
    
        public void OpenOptionsMenu()
        {
            SetMenu(Menu.Options);
        }
    
        public void OpenAudioMenu()
        {
            SetMenu(Menu.Audio);
        }
    
        public void OpenGraphicsMenu()
        {
            SetMenu(Menu.Graphics);
        }
    
        public void OpenKeybindingsMenu()
        {
            SetMenu(Menu.Keybindings);
        }

        public void OpenCreditsMenu()
        {
            SetMenu(Menu.Credits);
        }

        public void OpenHostScreen()
        {
            playAs.SetActive(false);
            hostOrClient.SetActive(true);
            host.SetActive(true);
            client.SetActive(false);
        }

        public void OpenClientScreen()
        {
            playAs.SetActive(false);
            hostOrClient.SetActive(true);
            host.SetActive(false);
            client.SetActive(true);
        }

        public void BackToPlayAsScreen()
        {
            playAs.SetActive(true);
            hostOrClient.SetActive(false);
        }

        public void PlayGame()
        {
            //InputManager.Instance.playerInput.SwitchCurrentActionMap("Gameplay");
            animator.SetTrigger(Quit);
            StartCoroutine(LoadGameCoroutine());

            //AudioManager.Instance.PlaySoundTrackIntro();
        }
    
        private static IEnumerator LoadGameCoroutine()
        {
            yield return new WaitForSeconds(1);
            //SceneManager.LoadScene("InitialCutscene");
        }

        public void QuitGame()
        {
            animator.SetTrigger(Quit);
            Debug.Log("Quit game...");
            StartCoroutine(QuitCoroutine());
        }
    
        private static IEnumerator QuitCoroutine()
        {
            yield return new WaitForSeconds(1);
            Application.Quit();
        }

        //UI AUDIO
        public void PlayOverUIButtonSound()
        {
            //AudioManager.Instance.PlayOverUIButtonSound();
        }
        public void PlayClickUIButtonSound()
        {
            //AudioManager.Instance.PlayClickUIButtonSound();
        }

        //OPTION SETTINGS
        public void SetBackgroundVolume(float value)
        {
            //AudioManager.Instance.SetBackgroundVolume(value);
        }
        public void SetPlayerVolume(float value)
        {
            //AudioManager.Instance.SetPlayerVolume(value);
        }
        public void SetEnemyVolume(float value)
        {
            //AudioManager.Instance.SetEnemyVolume(value);
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        public void SetResolution(int resolutionIndex)
        {
            var resolution = _resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

    }
}
