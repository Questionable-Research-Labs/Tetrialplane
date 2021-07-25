using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    bool showingMenu = false;
    public GameObject menuCanvas;
    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        /* 
        {
            Debug.Log("paused");
            Time.timeScale = 0;

            if (!UnityEngine.XR.InputDevice.GetButtonDown("primaryButton"))
            {
                if (UnityEngine.XR.InputDevice.GetButtonDown("primaryButton"))
                {
                    
                }
            }
        }
        */
        bool triggerValue;
     

        var controllers = new List<UnityEngine.XR.InputDevice>();
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controllers);

        foreach (var device in controllers)
        {
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out triggerValue) && triggerValue)
            {
                if (showingMenu == false)
                {
                    Time.timeScale = 0;
                    menuCanvas.SetActive(true);
                    Debug.Log("Menu Showing Now");
                    showingMenu = true;
                }
            }
        }
    }

    public void ResumeGame()
    {
        showingMenu = false;
        Time.timeScale = 1;
        menuCanvas.SetActive(false);
    }
    public void ExitToMenu()
    {
        showingMenu = false;
        Time.timeScale = 1;
        menuCanvas.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
