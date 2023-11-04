using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class MouseAim : MonoBehaviour
{
    [SerializeField] int forceDownLimit = 3;
    [SerializeField] FlightBox flightbox;
    [SerializeField] float pitchDownStrength = 0.05f;
    [SerializeField] public Transform mouseAimTF;
    [SerializeField] public Transform cameraTF;
    
    [SerializeField] private Transform extendedPointTF;
    [SerializeField] public Transform flightBoxPointerTF;
    [SerializeField] private Camera flightCamera;

    [SerializeField] private RectTransform mouseReticule;
    [SerializeField] private RectTransform flightPointerUI;

    void Update()
    {
        OnMouseMove();
        UpdateMouseUI();
    }

    public void OnMouseMove()
    {
        float lookInputX = Input.GetAxis("Mouse X");
        float lookInputY = -Input.GetAxis("Mouse Y");

        if (!flightbox.lostControl)
        {
            mouseAimTF.Rotate(cameraTF.right, lookInputY, Space.World);
            mouseAimTF.Rotate(cameraTF.up, lookInputX, Space.World);
            if (flightbox.pitch < forceDownLimit)
            {
                mouseAimTF.Rotate(cameraTF.right, pitchDownStrength, Space.World);
            }
        }
        else
        {
            mouseAimTF.Rotate(Vector3.zero, Space.World);
        }
    }

    private void UpdateMouseUI()
    {
        mouseReticule.transform.position = flightCamera.WorldToScreenPoint(extendedPointTF.position);
        flightPointerUI.transform.position = flightCamera.WorldToScreenPoint(flightBoxPointerTF.position);
    }









}
