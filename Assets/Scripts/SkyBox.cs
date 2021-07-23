using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBox : MonoBehaviour
{
    public Material skyboxMaterial;
    private float lerp;
    float timeLeft;
    Color targetColor;
    void Update() {
        if (timeLeft <= Time.deltaTime) {
            skyboxMaterial.SetColor("_SkyTint",targetColor);
            
            targetColor = new Color(Random.value, Random.value, Random.value,1);
            timeLeft = 1.0f;
        } else {
            skyboxMaterial.SetColor("_SkyTint",Color.Lerp(skyboxMaterial.GetColor("_SkyTint"), targetColor, Time.deltaTime / timeLeft));
        
            timeLeft -= Time.deltaTime;
        }
    }
}