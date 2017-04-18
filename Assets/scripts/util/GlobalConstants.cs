using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants 
{
  public const float CellBaseSpawnTimeoutSeconds = 3.0f;

  public enum CellType
  {    
    BASE = 0,
    COLONY,
    DRONE,
    SOLDIER
  }
}
