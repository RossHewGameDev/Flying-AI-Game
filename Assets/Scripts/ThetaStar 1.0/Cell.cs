using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Resources read and watched (seperate from Academic refrences):  
/// https://news.movel.ai/theta-star/ (A valuable guide for introducing Theta* elements)
/// https://theory.stanford.edu/~amitp/GameProgramming/ (Help for understanding A* and good refrence for imlementation) 
/// https://youtu.be/-L-WgKMFuhE (Sebastian Lague's A* series was useful refrence for the basic layout in Unity)
/// https://johntgz.github.io/2020/08/31/theta_star/#enter-the-theta (Help for understanding A* and good refrence for imlementation) 

/// <summary>
/// Properties of the Cell Objects that populate the cell map
/// </summary>
public class Cell
{
    public Cell parentCell;

    public bool traversable;        // defines whether the cell can be pathed through
    public bool spawnable;
    public Vector3 worldPosition;   

    public int gridX;
    public int gridY;
    public int gridZ;

    public float gCost;
    public float hCost;

    /// <summary>
    /// The properties of each individual Cell that makes up the map
    /// </summary>
    /// <param name="_traversable"></param>
    /// <param name="_worldPosition"></param>
    /// <param name="_gridX"></param>
    /// <param name="_gridY"></param>
    /// <param name="_gridZ"></param>
    public Cell(bool _traversable, bool _spawnable, Vector3 _worldPosition, int _gridX, int _gridY, int _gridZ)
    {
        
        gridX = _gridX;
        gridY = _gridY;
        gridZ = _gridZ;
        traversable = _traversable;
        worldPosition = _worldPosition;
    }

    public float fCost // cell heuristics cost
    {
        get
        {
            return gCost + hCost;
        }
    }

}
