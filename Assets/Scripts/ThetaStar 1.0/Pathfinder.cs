using System;
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
/// The Theta Star pathfinder that returns the path generated by Theta star to a target.
/// </summary>
public class Pathfinder : MonoBehaviour
{
    [Tooltip("if you want an Improved any angle A* pathfinder, enable this.")]
    [SerializeField] bool AStarOnly;

    [Tooltip("The max distance Theta* will check for line of sight (WARNING: PERFORMANCE HEAVY) Low cost default = 20")]
    [SerializeField] int raycastMaxDistance = 60;

    [Tooltip("The untraversable physics layer")]
    public LayerMask untraversableMask;


    [SerializeField] private List<Cell> openSet;         // Pathfinding open set (The cells that need to be checked)
    private HashSet<Cell> closedSet;   // Pathfinding closed set (The cells that have already been checked)
    private CellMapping cellMap;  

    PathingManager pathingManager;  

    private void Awake()
    {
        pathingManager = GetComponent<PathingManager>();
        cellMap = GetComponent<CellMapping>();
    }

    /// <summary>
    /// The call to start finding a path.
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindFlightPath(startPos, targetPos));
    }

    /// <summary>
    /// The internal coroutine that finds the flight path.
    /// pathfinds using Theta* pathfinding (and any angle A* if theta is disabled)
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private IEnumerator FindFlightPath(Vector3 startPos, Vector3 targetPos)
    {

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Cell startingCell = cellMap.CellFromWorldPoint(startPos); // finding the cell from where the agent is starting in
        Cell targetCell = cellMap.CellFromWorldPoint(targetPos); // finding the final target end of the path
        
        if (targetCell.traversable) // if the target is inside the traversable area, pathfind to it
        {
            openSet = new List<Cell>();
            closedSet = new HashSet<Cell>();  // using a HashSet for memory optimisation (Heap memory)

            if (!startingCell.traversable)  // setting the starting cell of the agent to always be true since if the agent is there, it must be traversable temporarily 
            {
                startingCell.traversable = true;
            }
            openSet.Add(startingCell); // add the starting cell to the path

            while (openSet.Count > 0)   // while there are still unevaluated cells left, evaluate. 
            {
                Cell currentCell = openSet[0];

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentCell.fCost || openSet[i].fCost == currentCell.fCost && openSet[i].hCost < currentCell.hCost)
                    {
                        currentCell = openSet[i]; // get the lowest f cost cell
                    }
                }

                openSet.Remove(currentCell);  // remove cell from the open set
                closedSet.Add(currentCell);  // add to the closed set

                if (currentCell.traversable)
                {
                    if (currentCell == targetCell) // checks if the path has been completed 
                    {
                        pathSuccess = true; 

                        break;
                    }
                }
                else
                {
                    break; // break if pathfinding is not necissary for this cell (its untraversable)
                }

                foreach (Cell neighbouring in cellMap.FindNeighbours(currentCell)) // find all of the cells neighbours
                {
                    if (neighbouring.traversable == false || closedSet.Contains(neighbouring)) // add any untraversable neighbours and adds them to the closed set
                    {
                        continue;
                    }



                    //Path Theta*
                    if (currentCell.parentCell != null && lineOfSight(currentCell.parentCell, neighbouring) && !AStarOnly) // the parent of a node in Theta* does not have to be a neighbour of the node
                    {                                                                                                     // as long as there is a line-of-sight between the two nodes
                        if (currentCell.parentCell.gCost + GetDistance(currentCell.parentCell, neighbouring) < neighbouring.gCost || !openSet.Contains(neighbouring))
                        {
                            neighbouring.gCost = currentCell.parentCell.gCost + GetDistance(currentCell.parentCell, neighbouring); // the neighbouring cells g cost is the distance between the parent and it plus the parent cells g cost
                            neighbouring.hCost = GetDistance(neighbouring, targetCell); // the neighbouring cells h cost is the distance between it and the end of the path
                            neighbouring.parentCell = currentCell; // set the parent of the neighbour to this cell
                            if (!openSet.Contains(neighbouring))
                            {
                                openSet.Add(neighbouring); // add neighbour to open set
                            }
                        }
                    }
                    // view Theta* Psuedo Code here
                    #region ThetaStarPsuedo
                    /*
                                            --THETA STAR PSUEDO CODE--

                                        if lineOfSight(parent[s], s '):
                                            if g_cost[parent[s]] + cost(parent[s], s') < g_cost(s'):
                                                    g_cost[s'] = g_cost[parent[s]] + cost(parent(s),s')
                                                    parent[s'] = parent[s]
                                                    if (s' IN openlist):
                                                        openlist.remove(s')	
                                                    openlist.insert(s', g_cost[s'] + h_cost[s'])

                                        else: 
                                            if g_cost[s] + cost(s, s') < g_cost[s']:
                                                g_cost[s'] = g_cost[s] + cost(s,s')
                                                parent[s'] = s
                                                if (s' IN openlist):
                                                    open.remove(s')
                                                openlist.insert(s', g_cost[s'] + h_cost[s'])
                    */
                    #endregion




                    //Path A*
                    else
                    {
                        float movementCostToNeighbour = currentCell.gCost + GetDistance(currentCell, neighbouring); // standard any angle A* pathfinding

                        if (movementCostToNeighbour < neighbouring.gCost || !openSet.Contains(neighbouring))      // get any neighbour whos g.cost is lower and isnt in the open set
                        {
                            neighbouring.gCost = movementCostToNeighbour;
                            neighbouring.hCost = GetDistance(neighbouring, targetCell); // get h cost of the neighbour from distance from the end of the path
                            neighbouring.parentCell = currentCell;  // set the parent of the neighbour to this cell
                            if (!openSet.Contains(neighbouring))
                            {
                                openSet.Add(neighbouring); // add neighbour to open set
                            }
                        }
                    }
                }
            }
        }

        yield return null;
        if (pathSuccess) // if the path is successful Trace the path from start to finish
        {
            waypoints = TracePath(startingCell, targetCell);
        }
        pathingManager.FinishedProcessingPath(waypoints, pathSuccess); // report to the pathing manager that the path has finished processing
    }

    /// <summary>
    /// Get the distrance between 2 Cells and return it as a float
    /// </summary>
    /// <param name="cellA"></param>
    /// <param name="cellB"></param>
    /// <returns></returns>
    float GetDistance(Cell cellA, Cell cellB)
    {
        return Vector3.Distance(new Vector3(cellA.gridX, cellA.gridY, cellA.gridZ), new Vector3(cellB.gridX, cellB.gridY, cellB.gridZ));
    }

    /// <summary>
    /// Follow the path from end to start in cells and then return a path of waypoints (vector3's), reversed.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Vector3[] TracePath(Cell start,  Cell end)
    {
        List<Cell> path = new List<Cell>();

        Cell currentCell = end; // setting the first cell as the end of the path

        while (currentCell != start) // loop through until you hit the starting cell
        {
            path.Add(currentCell);                  // add the cell to path
            currentCell = currentCell.parentCell;  // go to that cell's parent
        }
        
        Vector3[] waypoints = SimplePath(path);
        Array.Reverse(waypoints);
        path.Reverse();         // reverse the waypoints to give them from start to end
        cellMap.path = path;   // Give cellMap a path so it can show a debug of it in scene 
        return waypoints;     // return the waypoints as a traced path
    }

    /// <summary>
    /// Takes the list of cells generated by the path trace and turns it into a Vector3 array of waypoints
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Vector3[] SimplePath(List<Cell> path)
    {
        List<Vector3> waypoints = new List<Vector3>(); // creating the waypoints array
        Vector3 directionOld = Vector3.zero; // setting the first old direction as 0,0,0

        for (int i = 1; i < path.Count; i++) // loop through all of the Cell array (path) 
        {
            Vector3 directionNew = new Vector3(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY, path[i - 1].gridZ - path[i].gridZ); // get the direction to the next cell position in the array from the current cell 
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition); // add the cells world position 
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray(); // return the array of waypoints
    }

    /// <summary>
    /// returns true if there is a line of sight between two cells
    /// </summary>
    /// <param name="cellA"></param>
    /// <param name="cellB"></param>
    /// <returns></returns>
    private bool lineOfSight(Cell cellA, Cell cellB)
    {
        Vector3 dir = cellA.worldPosition - cellB.worldPosition; // get the direction between two cells
        dir = -dir; // reverse to draw a line between them

        dir.Normalize();
        if (!Physics.Raycast(cellA.worldPosition, dir, raycastMaxDistance))  //raycasts to max distance to see if theres a line of sight
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
        //
    }


}

