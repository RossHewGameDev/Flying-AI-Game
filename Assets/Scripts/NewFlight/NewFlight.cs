using System.ComponentModel;
using System.Drawing;
using UnityEngine;

public class NewFlight : MonoBehaviour
{
    [Header("Rigidbody")]
    public GameObject flightObject;
    private Rigidbody flightRB;

    [Header("Flight Target")]
    [SerializeField] Transform flightTargetTF;
    [SerializeField] MouseControl mouseControl;



    [Header("Speed")]
    public float speed;
    [Tooltip("If you want the actual speed of the flyer, get rigidbody velocity. dont use this.")]
    public float flightSpeed_Current;
    public float flight_Speed_Max { get { return flightSpeed_Max; } }
    private float flightSpeed_Max = 150;


    [Header("Pitch,Yaw,Roll")]
    public int pitch_Degrees;

    [Tooltip("Max Angle of turn into target")]
    [SerializeField] float aggressiveTurnAngle = 10f;
    [SerializeField] float sensitivity = 1f;


    [SerializeField][Range(-1f, 1f)] public float pitch_Clamped = 0f;
    [SerializeField][Range(-1f, 1f)] public float yaw_Clamped = 0f;
    [SerializeField][Range(-1f, 1f)] public float roll_Clamped = 0f;
    [Tooltip("Pitch, Yaw, Roll")] 
    public Vector3 turnTorque = new Vector3(90f, 25f, 100f);

    public float pitch_Power { set { pitch_Clamped = Mathf.Clamp(value, -1f, 1f); } get { return pitch_Clamped; } }
    public float yaw_Power { set { yaw_Clamped = Mathf.Clamp(value, -1f, 1f); } get { return yaw_Clamped; } }
    public float roll_Power { set { roll_Clamped = Mathf.Clamp(value, -1f, 1f); } get { return roll_Clamped; } }

    void Start()
    {
        flightRB = flightObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        mouseRotate();
        movement();
    }

    private void movement()
    {
        pitch_Degrees = (int)Mathf.DeltaAngle(0, flightObject.transform.eulerAngles.x); // Calculate the pitch in degrees relative to the horizontal.

        flightSpeed_Current = Mathf.Lerp(speed, flightSpeed_Max, 0.2f * Time.deltaTime); // Smoothly adjust to the Speed Target.

        flightRB.AddForce(flightObject.transform.forward * flightSpeed_Current, ForceMode.Acceleration); // Apply forward force to the flight object.
    }

    private void mouseRotate()
    {

        Vector3 localFlyTarget = flightObject.transform.InverseTransformPoint(flightTargetTF.position).normalized * sensitivity; // getting the flight target 

        float angleOffTarget = Vector3.Angle(flightObject.transform.forward, flightTargetTF.position - flightObject.transform.position) ;
        float agressiveRoll = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        float wingsLevelRoll = flightObject.transform.right.y;
        float wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);

        yaw_Power = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        pitch_Power = -Mathf.Clamp(localFlyTarget.y, -1f, 1f);
        roll_Power = Mathf.Lerp(wingsLevelRoll, agressiveRoll, wingsLevelInfluence);

        flightRB.AddRelativeTorque(new Vector3(turnTorque.x * pitch_Power, turnTorque.y * yaw_Power, -turnTorque.z * roll_Power) * 1f, ForceMode.Force);
    }

}
