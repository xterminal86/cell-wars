using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants 
{
  public const float DroneSpawnTimeSeconds = 1.0f;
  public const float SoldierSpawnTimeSeconds = 3.0f;
  public const float AttackTimeout = 0.5f;

  public const int SoldiersPerBarrack = 2;
  public const int BuildRangeDistance = 3;

  public const int ColonyDronesCost = 8;
  public const int BarracksDronesCost = 16;

  public const int CellBaseHitpoints = 12;
  public const int CellColonyHitpoints = 8;
  public const int CellBarracksHitpoints = 6;
  public const int CellSoldierHitpoints = 4;
  public const int CellDroneHitpoints = 1;

  public static Dictionary<CellType, Color> PlayerColors = new Dictionary<CellType, Color>()
  {
    { CellType.BASE, Color.green },
    { CellType.COLONY, Color.green },
    { CellType.DRONE, Color.green },
    { CellType.BARRACKS, Color.red },
    { CellType.SOLDIER, Color.red }
  };

  public static Dictionary<CellType, Color> CPUColors = new Dictionary<CellType, Color>()
  {
    { CellType.BASE, Color.cyan },
    { CellType.COLONY, Color.cyan },
    { CellType.DRONE, Color.cyan },
    { CellType.BARRACKS, Color.magenta },
    { CellType.SOLDIER, Color.magenta }
  };

  public static List<Dictionary<CellType, Color>> ColorsList = new List<Dictionary<CellType, Color>>()
  {
    PlayerColors, CPUColors
  };

  public enum CellType
  {    
    BASE = 0,
    COLONY,
    DRONE,
    BARRACKS,
    SOLDIER
  }
}
