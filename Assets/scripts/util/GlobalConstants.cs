using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants 
{
  public const float CellBaseSpawnTimeoutSeconds = 1.0f;

  public const int ColonyDronesCost = 8;
  public const int BarracksDronesCost = 16;
  public const int BuildRangeDistance = 3;

  public enum CellType
  {    
    BASE = 0,
    COLONY,
    DRONE,
    SOLDIER
  }
}
