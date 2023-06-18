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
        public static OptionsManager instance { get; private set; }
        
        private Resolution[] _resolutions;
        private List<string> _options = new List<string>();
        private int _currentResolutionIndex;

        private string _playerName;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }

            
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
            PlayerPrefs.SetString("playerName", str);
        }
    }
}
