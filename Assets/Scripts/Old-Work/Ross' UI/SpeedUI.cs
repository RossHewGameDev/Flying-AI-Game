using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeedUI : MonoBehaviour
{
    [SerializeField] TMP_Text speedText;
    [SerializeField] Slider slider;

    [SerializeField] FlightBox flightBox;
      

    // Update is called once per frame
    void Update()
    {
        speedUpdateUI();
    }



    private void speedUpdateUI()
    {
        slider.value = flightBox.forwardSpeed;
        speedText.text = ((int)flightBox.forwardSpeed).ToString() + "Mph";
    }








}
