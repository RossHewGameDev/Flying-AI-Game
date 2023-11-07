using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{

    // fix this assignment mess.
    [SerializeField] NewFlight flight;
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


        mouseAimTF.Rotate(cameraTF.right, lookInputY, Space.World);
        mouseAimTF.Rotate(cameraTF.up, lookInputX, Space.World);
    }

    private void UpdateMouseUI()
    {
        mouseReticule.transform.position = flightCamera.WorldToScreenPoint(extendedPointTF.position);
        flightPointerUI.transform.position = flightCamera.WorldToScreenPoint(flightBoxPointerTF.position);
    }
}
