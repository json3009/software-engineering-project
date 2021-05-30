using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicsMenu : MonoBehaviour
{
    Resolution[] resolutions;

    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown graphicsDropdown;

    int currentQualityLevel;


    private void Start()
    {
        //get all resolutions and store in our array
        resolutions = Screen.resolutions;

        //clear out the default options from our dropdown
        resolutionDropdown.ClearOptions();

        //list of strings
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        currentQualityLevel = QualitySettings.GetQualityLevel();

        //loop through every resolution element in our array 
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;

            //add option to our options list
            options.Add(option);

            //check if current resolution is equal to our resolution
            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        //add options to the dropdown
        resolutionDropdown.AddOptions(options);
        //set current value
        resolutionDropdown.value = currentResolutionIndex;
        graphicsDropdown.value = currentQualityLevel;
        //display correct value
        resolutionDropdown.RefreshShownValue();
        graphicsDropdown.RefreshShownValue();
    }

    public void SetResolution (int resolutionIndex)
    {
        //get the resolution from the resolutions array, that we want to use
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality (int qualityIndex)
    {

        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

}
