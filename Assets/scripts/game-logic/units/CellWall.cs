using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellWall : CellBaseClass 
{
  public CellWall()
  {
    Type = GlobalConstants.CellType.WALL;
    Hitpoints = GlobalConstants.CellWallHitpoints;
    Priority = GlobalConstants.CellWallPriority;
  }
}
