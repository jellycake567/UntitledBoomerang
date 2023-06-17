using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject mainMenu;
    //public GameObject options;
    //public GameObject credits;

    //public GameObject controls;
    //public GameObject graphics;
    //public GameObject _audio;

    [Header("Options")]
    //public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    //public Slider volumeSlider;

    float multiplier = 30f;
    float currentVolume;
    List<Resolution> resolutions = new List<Resolution>();


    void Start()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        Resolution[] tempResolution = Screen.resolutions; // Grab all resolutions
        Array.Reverse(tempResolution);

        int currentResolutionIndex = 0;

        // Loop through resolutions
        for (int i = 0; i < tempResolution.Length; i++)
        {
            if (tempResolution[i].refreshRate == 60)
            {
                // Add Resolution
                resolutions.Add(tempResolution[i]);

                // Add resolution string to drop down list
                string option = tempResolution[i].width + " x " + tempResolution[i].height;
                options.Add(option);

                // If resolution matches, select that resolution
                if (tempResolution[i].width == Screen.currentResolution.width && tempResolution[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);
    }

    public void SetVolume(float volume)
    {
        //audioMixer.SetFloat("Master", Mathf.Log10(volume) * multiplier);
        currentVolume = volume;
    }

    public void SetFullscreenMode(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", Convert.ToInt32(Screen.fullScreen));
        PlayerPrefs.SetFloat("VolumePreference", currentVolume);
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        // Resolution
        if (PlayerPrefs.HasKey("ResolutionPreference"))
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference");
        else
            resolutionDropdown.value = currentResolutionIndex;

        // Fullscreen
        if (PlayerPrefs.HasKey("FullscreenPreference"))
            Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));
        else
            Screen.fullScreen = true;

        // Volume
        //if (PlayerPrefs.HasKey("VolumePreference"))
        //    volumeSlider.value = PlayerPrefs.GetFloat("VolumePreference");
        //else
        //    volumeSlider.value = PlayerPrefs.GetFloat("VolumePreference");
    }

    #region Buttons

    public void Play()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1;
    }

    public void Menu()
    {
        mainMenu.SetActive(true);
        //options.SetActive(false);
        //credits.SetActive(false);

        SaveSettings();
    }

    public void Options()
    {
        mainMenu.SetActive(false);
        //options.SetActive(true);
        //
        //controls.SetActive(false);
        //graphics.SetActive(false);
        //_audio.SetActive(false);
    }

    public void Controls()
    {
        //controls.SetActive(true);
        //graphics.SetActive(false);
        //_audio.SetActive(false);
    }

    public void Graphics()
    {
        //controls.SetActive(false);
        //graphics.SetActive(true);
        //_audio.SetActive(false);
    }

    public void Audio()
    {
        //controls.SetActive(false);
        //graphics.SetActive(false);
        //_audio.SetActive(true);
    }

    public void Credits()
    {
        mainMenu.SetActive(false);
        //credits.SetActive(true);
    }


    public void ExitGame()
    {
        Application.Quit();
    }

    public void BacktoMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    #endregion
}
