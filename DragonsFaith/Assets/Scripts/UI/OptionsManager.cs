using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class OptionsManager : MonoBehaviour
    {
        public static OptionsManager Instance { get; private set; }
        
        private Resolution[] _resolutions;
        private List<string> _options = new List<string>();
        private int _currentResolutionIndex;

        private string _playerName;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource backgroundMusic;
        [SerializeField] private AudioSource uiSound;
        [SerializeField] private AudioSource enemySound;
        [SerializeField] private AudioSource playerSound;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            DontDestroyOnLoad(this);
            
            var resolutions = Screen.resolutions.Select(resolution => new Resolution
            {
                width = resolution.width, height = resolution.height
            }).Distinct();
            _resolutions = resolutions as Resolution[] ?? resolutions.ToArray();
        
            _options = new List<string>();
            _currentResolutionIndex = 0;
            for (var i = 0; i < _resolutions.Length; i++)
            {
                var option = _resolutions[i].width + "x" + _resolutions[i].height;
                _options.Add(option);
    
                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    _currentResolutionIndex = i;
                }
            }
        }

        public void SetDropdown(Dropdown dropdown)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(_options);
            dropdown.value = _currentResolutionIndex;
            dropdown.RefreshShownValue();
        }

        public static void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }
        
        public static void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
        
        public void SetResolution(int resolutionIndex)
        {
            var resolution = _resolutions[resolutionIndex];
            _currentResolutionIndex = resolutionIndex;
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void SavePlayerName(string str)
        {
            _playerName = str;
        }
        
        public string RetrievePlayerName()
        {
            return _playerName;
        }
        
        //TODO: add audio sources
        public void SetPlayerVolume(float value)
        {
            //playerSound.volume = value;
        }
        public float GetPlayerVolumeSound()
        {
            //return playerSound.volume;
            return 0;
        }
        
        public void SetEnemyVolume(float value)
        {
            //enemySound.volume = value;
        }
        public float GetEnemyVolumeSound()
        {
            //return enemySound.volume;
            return 0;
        }
        
        public void SetBackgroundVolume(float value)
        {
            //backgroundMusic.volume = value;
        }
        public float GetBackgroundVolumeSound()
        {
           //return backgroundMusic.volume;
           return 0;
        }
    }
}
