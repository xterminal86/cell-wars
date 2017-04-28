using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drone - occupies grid cell, does nothing. 
/// Used in construction and spawning of other cells.
/// </summary>
public class CellDrone : CellBaseClass 
{
  public CellDrone()
  {
    Hitpoints = GlobalConstants.CellDroneHitpoints;
  }
}
