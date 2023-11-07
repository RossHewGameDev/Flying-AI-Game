using System.Collections;
using System.Collections.Generic;
using UnityEngine; // keeping these around for later functionality.

public class Pilot : NewFlight
{
    [Header("Pilot AI Settings")]
    public bool isAI;

    [HideInInspector]
    public Vector3 flightDirection;
    void Update()
    {
        EngineOn();
    }

    /// <summary>
    /// The aircraft is active and flying.
    /// </summary>
    private void EngineOn()
    {
        FlightThrust();

        if (isAI)
        {
            FlightRotateTowards(flightDirection);
        }
        else
        {
            FlightRotateTowardsMouse();
        }
    }
}
