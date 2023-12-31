    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Resources read and watched (seperate from Academic refrences):  
/// https://news.movel.ai/theta-star/ (A valuable guide for introducing Theta* elements)
/// https://theory.stanford.edu/~amitp/GameProgramming/ (Help for understanding A* and good refrence for imlementation) 
/// https://youtu.be/-L-WgKMFuhE (Sebastian Lague's A* video was useful refrence for the basic layout in Unity)
/// https://johntgz.github.io/2020/08/31/theta_star/#enter-the-theta (Help for understanding A* and good refrence for imlementation) 
/// https://youtu.be/dn1XRIaROM4 (Sebastai Lague's A* unit example was used with adjustments to help produce the basic path for the agents - doing it from scratch was quite hard)

/// <summary>
/// The Agent that follows the path generated by the Theta Star pathfinder. 
/// Regenerates the path with information handed to it via the statemachine
/// </summary>
public class ThetaAgent : MonoBehaviour
{

    public Transform target; //The end flight target that the pathfinder is pathfinding towards
    [SerializeField] CellMapping cellMapping; 
    [Tooltip("The complete radius to the next cell in the path")]
    [SerializeField] float completePosRadius;
    [Tooltip("Shows whether the path has completed")]
    [SerializeField] private bool pathComplete;
    [Tooltip("The Untraversable physics layer")]
    [SerializeField] LayerMask untraversable;


    Vector3 debugWaypoint;

    Statemachine statemachine;
    Pilot pilot;
    
    private Vector3[] path;             // the path generated by the pathfinder 
    private int targetIndex;           // current cell in the index that the agent is trying to fly to
    
    private bool fleeChkRunning;            // keeps track of the flee coroutine and if its running
    private bool pathError;                // check for if there has been a pathing error
    private bool proximityRunning;        // keeps track of the proximity coroutine and if its running

    Coroutine followingPath;
    Coroutine fleeingChk;
    Coroutine proximityCP;


    // gets the lookahead distance relative to the cell size
    private float LookaheadDistance { get { return cellMapping.cellRadius * 5; } }

    private void Start()
    {
        PathingManager.RequestPath(transform.position, target.position, OnPathFound); // getting the starting path
        pilot = GetComponent<Pilot>(); // new "pilot" removes the need for this here (Implementation later.)
        statemachine = GetComponent<Statemachine>();
        fleeChkRunning = false;
    }

    private void Update()
    {
        if (pathComplete) // creates a new path when one is completed
        {
            statemachine.getFlightTarget(); // gets a new flight target position
            PathingManager.RequestPath(transform.position, target.position, OnPathFound); // requests a new path to flight target

            /// --- Quick Fix for the stuck in space where the AI shouldnt be
            pilot.FlightThrust(); // pushes the glider forward for one tick
            ///
        }

        if (!fleeChkRunning || pathError) // Makes sure that only 1 coroutine instance is running at a time.
        {
            if (fleeingChk != null)
            {
                StopCoroutine(fleeingChk); // stops the flee check if one is currently running 
                pathError = false;        
            }
            fleeingChk = StartCoroutine(FleeingChk());
        }

        if (!proximityRunning) // Makes sure that only 1 coroutine instance is running at a time.
        {
            if (proximityCP != null)
            {
                StopCoroutine(proximityCP); // stops any current proximity checks
            }
            proximityCP = StartCoroutine(proximityChangePath());
        }

    }

    /// <summary>
    /// Checks if there is an untraversable collider in front of the Agent, and repaths if there is
    /// Runs every 2 seconds to save on performance
    /// </summary>
    /// <returns></returns>
    private IEnumerator proximityChangePath()
    {
        proximityRunning = true;

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, LookaheadDistance, untraversable)) // should avoid them from getting stuck on objects
        {
            Debug.DrawRay(transform.position, transform.forward.normalized * LookaheadDistance, Color.red);
            StopCoroutine(followingPath);
            statemachine.getFlightTarget();
            PathingManager.RequestPath(transform.position, target.position, OnPathFound);
        }
        yield return new WaitForSeconds(2f);

        proximityRunning = false;
    }

    /// <summary>
    /// Checks to see if the player is close and then repaths when the player comes into range.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FleeingChk()
    {
        fleeChkRunning = true;

        if (statemachine.mindState == MindState.Fleeing)
        {
            StopCoroutine(followingPath);
            statemachine.getFlightTarget();
            PathingManager.RequestPath(transform.position, target.position, OnPathFound);
            pilot.FlightThrust();

            yield return new WaitUntil(fleeingChange); // waits for when the mindstate is not fleeing
        }

        fleeChkRunning = false;
        yield return null;
    }

    /// <summary>
    /// Checks if the mindstate is not fleeing and then returns true
    /// </summary>
    /// <returns></returns>
    private bool fleeingChange()
    {
        if (statemachine.mindState != MindState.Fleeing)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// When a path is found and is successful, set agent to follow it. 
    /// If a successful path is not found, there is a path error.
    /// </summary>
    /// <param name="newPath"></param>
    /// <param name="pathSuccessful"></param>
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath; // sets the new path
            if (followingPath != null)
            {
                StopCoroutine(followingPath);
            }
            pathComplete = false;
            followingPath = StartCoroutine(FollowPath());
        }
        else
        {
            pathError = true;
        }
    }

    /// <summary>
    /// Follows the generated path by travelling from cell to cell
    /// </summary>
    /// <returns></returns>
    private IEnumerator FollowPath()
    {
        
        targetIndex = 0; // The current cell index in the path
        Vector3 currentWaypoint = path[0];
        debugWaypoint = currentWaypoint;
        while (true)
        {
            

            if (hasReachedPosition(transform.position,currentWaypoint)) // checks to see if the agent has reached the current waypoint
            {

                targetIndex++;  // moves onto the next cell in the path
                if (targetIndex >= path.Length) 
                {
                    pathComplete = true; // completes the path when the index equals the paths length
                    yield break;
                }
                currentWaypoint = path[targetIndex]; // moves along the path
                debugWaypoint = currentWaypoint;
            }
            //FLYING ---------

            // -- REWRITE THIS ENTIRE CLASS -- //

            pilot.flightDirection = currentWaypoint;

            //FLYING --------- 
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// Checks to see if the agent has reached the target within a complete radius. 
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool hasReachedPosition(Vector3 agent, Vector3 target)
    {

        if (target.x - agent.x < completePosRadius && target.y - agent.y < completePosRadius && target.z - agent.z < completePosRadius)
        {
            return true;

        }
        else
        {
            return false;
        }
    }






    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, debugWaypoint);
    }

}
