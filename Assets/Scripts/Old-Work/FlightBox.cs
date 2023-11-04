using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;

public class FlightBox : MonoBehaviour
{
    
    [Tooltip("DO NOT CHANGE")]
    public float forwardSpeed = 28f; //Dont Change me

    [Tooltip("Cinemachine virtual camera being used")]
    [SerializeField] CinemachineVirtualCamera cinemachine;

    [Tooltip("The size in the center of the screen that activates AutoLevel")]
    [SerializeField] float autoLevelDeadzone;

    [Tooltip("Rigidbody of glider")]
    [SerializeField] public Rigidbody flightRB;

    [Tooltip("The amount in degrees that autolevel tries to make the glider roll when banking side to side (should be a miuns number)")]
    [SerializeField] float bankAmount;

    [Tooltip("Strength of the slowing when glider pulls up")]
    [SerializeField] float slowDown;

    [Tooltip("The minimum speed of the glider")]
    [SerializeField] float minimumSpeed;

    [Tooltip("Gliders top speed")]
    [SerializeField] float glideSpeedMax;

    [Tooltip("the angle at which the glider will start to slow down (reccomended -1)")]
    [SerializeField] int angleOfSlowdown;

    [Tooltip("The cooldown of the flap in seconds")]
    [SerializeField] public float flapCooldown;

    [Tooltip("The strength of the rotations for rolling (reccomended between 12 - 20)")]
    [SerializeField] float manualRotationPower = 0.1f;

    [Header("Glide Speed & angles")]
    [SerializeField] int slowSpeedAngle;
    [SerializeField] int mediumSpeedAngle;

    [Tooltip("after collision wait time")]
    [SerializeField] int collisionWait;

    [Header("speed stuff")]
    [Tooltip("speed to be taken away from gliders top speed when in slow mode")]
    [SerializeField] float slowSpeed;
    [Tooltip("speed to be added to gliders top speed when in fast mode")]
    [SerializeField] float fastSpeed;

    [SerializeField] float stunnedSpeed = 5f;

    [Header("Flight Target")]
    [SerializeField] Transform flightTargetTF;
    [SerializeField] MouseAim mouseAim;

    [Header("RotationStuffs")]
    [Tooltip("Pitch, Yaw, Roll")] public Vector3 turnTorque = new Vector3(90f, 25f, 100f);
    [SerializeField] [Range(-1f, 1f)] public float pitchr = 0f;
    [SerializeField] [Range(-1f, 1f)] public float yawr = 0f;
    [SerializeField] [Range(-1f, 1f)] public float rollr = 0f;
    [Tooltip("Max Angle of turn into target")] public float aggressiveTurnAngle = 10f;
    [SerializeField] float sensitivity = 1f;

    [SerializeField] public Image flapCooldownSprite;


    public int pitch; // pitch of the flight box
    public bool lostControl;
    private float glideSpeed;
    private Vector3 collisionPosition;
    private GameObject cMachineObj;
    private int collisionRotatePower = 16;

    [Header("Debug View")]
    public bool slowingActive;

    public float Pitch { set { pitchr = Mathf.Clamp(value, -1f, 1f); } get { return pitchr; } }
    public float Yaw { set { yawr = Mathf.Clamp(value, -1f, 1f); } get { return yawr; } }
    public float Roll { set { rollr = Mathf.Clamp(value, -1f, 1f); } get { return rollr; } }

    void Start()
    {
        cMachineObj = cinemachine.LookAt.gameObject;
        lostControl = false;
    }

    private void FixedUpdate()
    {
        ManualRotate();
        mouseRotate();
        movement();
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisionPosition = -collision.collider.ClosestPoint(gameObject.transform.position);

        if (!lostControl)
        {
            StartCoroutine(LostControl());
        }
    }

    /// <summary>
    /// movement contains the basic aspects of forward motion of the glider;
    /// adding any other force should be done elsewhere;
    /// Consider movement as the kinetic energy of the glider;
    /// </summary>
    private void movement()
    {
        pitch = (int)Mathf.DeltaAngle(0, transform.eulerAngles.x);

        if (!lostControl)
        {
            if (pitch < angleOfSlowdown)
            {
                forwardSpeed = Mathf.Lerp(forwardSpeed, minimumSpeed, Mathf.Lerp(0, slowDown * Time.deltaTime, -pitch * 0.14f)); // slowing down to min speed
                slowingActive = true;
            }

            else
            {
                glideSpeed = glideSpeedMax; // setting max glide speed;

                if (pitch > 0 && pitch < slowSpeedAngle)
                {
                    forwardSpeed = Mathf.Lerp(forwardSpeed, glideSpeed - slowSpeed, 0.2f * Time.deltaTime); // slow speed

                }
                if (pitch > slowSpeedAngle && pitch < mediumSpeedAngle)
                {
                    forwardSpeed = Mathf.Lerp(forwardSpeed, glideSpeed, 0.2f * Time.deltaTime); // normal speed 

                }
                if (pitch > mediumSpeedAngle)
                {
                    forwardSpeed = Mathf.Lerp(forwardSpeed, glideSpeed + fastSpeed, 0.2f * Time.deltaTime); // fast speed

                }

                slowingActive = false;
            }
        }

        else
        {
            forwardSpeed = stunnedSpeed; // when glider is stunned speed is set to this
        }

        flightRB.AddForce(transform.forward * forwardSpeed, ForceMode.Acceleration);
    }

    /// <summary>
    /// mouseRotate controls player input and rotation;
    /// adds torque (rotational force) relative to mouse position;
    /// </summary>
    private void mouseRotate()
    {

        Vector3 localFlyTarget = transform.InverseTransformPoint(flightTargetTF.position).normalized * sensitivity; // getting the flight target 

        float angleOffTarget = Vector3.Angle(transform.forward, (flightTargetTF.position - transform.position));
        float agressiveRoll = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        float wingsLevelRoll = transform.right.y;
        float wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);

        yawr = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        pitchr = -Mathf.Clamp(localFlyTarget.y, -1f, 1f);
        rollr = Mathf.Lerp(wingsLevelRoll, agressiveRoll, wingsLevelInfluence);

        flightRB.AddRelativeTorque(new Vector3(turnTorque.x * pitchr, turnTorque.y * yawr, -turnTorque.z * rollr) * 1f, ForceMode.Force);
    }


    private void ManualRotate()
    {
        if (Input.GetKey(KeyCode.A) && !lostControl)
        {
            flightRB.AddRelativeTorque(0, 0, manualRotationPower, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.D) && !lostControl)
        {
            flightRB.AddRelativeTorque(0, 0, -manualRotationPower, ForceMode.Acceleration);
        }
        if (lostControl)
        {
            flightRB.AddRelativeTorque(0, 0, collisionRotatePower, ForceMode.Force);
        }
    }

    private IEnumerator LostControl()
    {
        cinemachine.LookAt = mouseAim.flightBoxPointerTF;
        lostControl = true;

        flightRB.AddForce(collisionPosition, ForceMode.Impulse);

        yield return new WaitForSecondsRealtime(collisionWait);

        lostControl = false;

        cinemachine.LookAt = cMachineObj.transform;
    }
}
