using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Resources read and watched (seperate from Academic refrences):  
/// https://news.movel.ai/theta-star/ (A valuable guide for introducing Theta* elements)
/// https://theory.stanford.edu/~amitp/GameProgramming/ (Help for understanding A* and good refrence for imlementation) 
/// https://youtu.be/-L-WgKMFuhE (Sebastian Lague's A* series was useful refrence for the basic layout in Unity)
/// https://johntgz.github.io/2020/08/31/theta_star/#enter-the-theta (Help for understanding A* and good refrence for imlementation) 

/// <summary>
/// Generates the cell map which is used by the Theta Star Algorithm to find untraversable areas to pathfind around.
/// </summary>
public class CellMapping : MonoBehaviour
{
    public Transform player;
    public LayerMask untraversableMask;

    public Vector3 worldSize;   // size of the world that the map generates in
    public float cellRadius;   // radius of the cells that populate the map

    public Cell[,,] cellMap; // the overall map 

    [HideInInspector]public List<Cell> path;     // the path that has been generated (inserted in here so we can debug it and see the gizmos draw the path)

    private float cellDiameter;                 
    private int c_Width, c_Height, c_Length;  // cell number in width, height, length
    private Vector3 startPoint;              // the start point of the cell map

    private void Awake()
    {
        cellDiameter = cellRadius * 2;
        c_Width = (int)(worldSize.x / cellDiameter);    // getting the number of cells in Width
        c_Height = (int)(worldSize.y / cellDiameter);  // getting the number of cells in Height
        c_Length = (int)(worldSize.z / cellDiameter); // getting the number of cells in Length

        InitMap(); //Initilizing the cell map

    }
    /// <summary>
    /// grid initilization started here
    /// collision check also takes place in here but this might be changed 
    /// Modified from Sebastian lagues tutorial (https://youtu.be/-L-WgKMFuhE) 
    /// </summary>
    private void InitMap()
    {
        // setting cell number and cellMap size (resolution)
        cellMap = new Cell[c_Width, c_Height, c_Length];
        // setting start point to where the bottom left of the grid would start generating from.
        startPoint = transform.position - (Vector3.right * worldSize.x * 0.5f) - (Vector3.up * worldSize.y * 0.5f) - (Vector3.forward * worldSize.z * 0.5f);


        for (int x = 0; x < c_Width; x++)           // looping through each axis to populate the cellmap with cells
        {
            for (int y = 0; y < c_Height; y++)
            {
                for (int z = 0; z < c_Length; z++)
                {
                    // we add the x y z components to each cell
                    Vector3 position = startPoint + Vector3.right * (x * cellDiameter + cellRadius) + Vector3.up * (y * cellDiameter + cellRadius) + Vector3.forward * (z * cellDiameter + cellRadius);
                    // we then check to see if this cell is traversable (It used to be checking with the cell radius)
                    // checking with cell diameter means we see if theres an object anywhere near this cell
                    bool spawnable;
                    bool traversable = !Physics.CheckSphere(position, 1, untraversableMask);
                    ///adding in check for if spawning system can use area as a spawning location.
                    if (traversable && y == c_Height * 0.5f && Physics.CheckSphere(position, 1))
                    {
                        spawnable = !Physics.CheckSphere(position, untraversableMask);
                    }
                    else
                    {
                        spawnable = false;
                    }

                    cellMap[x, y, z] = new Cell(traversable, position, x, y, z); /// this has had a 0 added to the Cell properties to set all tempratures to 0 fo
                }
            }
        }
    }
    /// <summary>
    /// Grabs a cell location (vector3) in world space 
    /// Modified from Sebastian lagues tutorial (https://youtu.be/-L-WgKMFuhE)
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Cell CellFromWorldPoint(Vector3 worldPos)
    {
        float percentX = AxisPercentage(worldPos, "x");
        float percentY = AxisPercentage(worldPos, "y");
        float percentZ = AxisPercentage(worldPos, "z");

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        percentZ = Mathf.Clamp01(percentZ);

        int x = (int)(c_Width * percentX);
        int y = (int)(c_Height * percentY);
        int z = (int)(c_Length * percentZ);

        return cellMap[x, y, z];
    }

    public float AxisPercentage(Vector3 worldPos, string Axis)
    {
        if (Axis == "x")
        {
            return (worldPos.x + worldSize.x * 0.5f) / worldSize.x;
        }
        if (Axis == "y")
        {
            return (worldPos.y + worldSize.y * 0.5f) / worldSize.y;
        }
        if (Axis == "z")
        {
            return (worldPos.z + worldSize.z * 0.5f) / worldSize.z;
        }
        else
        {
            Debug.Log("AXIS IS NOT VALID, Please enter x, y or z");
            return 0;
        }
    }

    /// <summary>
    /// adds all cells as neighbours within 1 cell radius 
    /// Modified from Sebastian lagues tutorial (https://youtu.be/-L-WgKMFuhE)
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public List<Cell> FindNeighbours(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();

        //loops through the cells that are 1 away from the target cell
        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0)
                        continue;

                    int checkX = cell.gridX + x; // get the cells coordinates from the cell map
                    int checkY = cell.gridY + y;
                    int checkZ = cell.gridZ + z;

                    if (checkX >= 0 && checkX < c_Width)
                    {
                        if (checkY >= 0 && checkY < c_Height)
                        {
                            if (checkZ >= 0 && checkZ < c_Length)
                            {
                                // add the neighbour Cell to the cell map 
                                neighbours.Add(cellMap[checkX, checkY, checkZ]);
                            }
                        }
                    }
                }
            }
        }
        return neighbours; 
    }






    /// <summary>
    /// Display and debug visuals for the in scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, worldSize); //drawing the world size (bounds of the AI)

        if (cellMap != null)
        {
            Cell playersCurrentCell = CellFromWorldPoint(player.position); 


            foreach (Cell currentCell in cellMap)
            {

                if (!currentCell.traversable) // shows all untraversable cells with red wirecubes
                {
                    //Gizmos.color = Color.red;
                    //Gizmos.DrawWireCube(currentCell.worldPosition, new Vector3(cellDiameter, cellDiameter, cellDiameter));
                }
                else
                {
                    if (path != null)
                    {
                        if (path.Contains(currentCell)) // shows the path with green cubes
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawWireCube(currentCell.worldPosition, new Vector3(cellDiameter, cellDiameter, cellDiameter));
                        }
                    }
                    if (playersCurrentCell == currentCell) // shows current player cell (or nearest cell)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(currentCell.worldPosition, new Vector3(cellDiameter, cellDiameter, cellDiameter));
                    }
                }
            }
        }
    }

}
