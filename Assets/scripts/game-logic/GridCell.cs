using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to represent map (2D array of GridCells) 
/// </summary>
public class GridCell
{
  public Int2 Coordinates = Int2.Zero;

  // If multiple holders influence the same cell, it should be accounted for
  public int NumberOfLocks = 0;
}
