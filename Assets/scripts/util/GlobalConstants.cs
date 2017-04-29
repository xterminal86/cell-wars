using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants 
{
  // Time between new cell is spawned by building
  public const float DroneSpawnTimeSeconds = 1.0f;
  public const float SoldierSpawnTimeSeconds = 4.0f;

  // Attacker cooldown in seconds
  public const float AttackTimeout = 1.5f;

  // Various game parameters
  public const float BulletSpeed = 2.0f;
  public const float AttackerMoveSpeed = 0.5f;
  public const float HolderSlowFactor = 0.5f;
  public const float CameraZoomSpeed = 1.0f;
  public const float CameraMoveSpeed = 10.0f;
  public const float CellHolderRange = 3.0f;

  // Cooldown between build actions for CPU to give player some advantage
  public const float CPUActionTimeout = 6.0f;

  public const int SoldiersPerBarrack = 2;

  // Maximum block distance between already built building 
  // and new spot for a new building.
  public const int BuildRangeDistance = 3;

  // Various cells hitpoints
  public const int CellBaseHitpoints = 32;
  public const int CellColonyHitpoints = 8;
  public const int CellBarracksHitpoints = 8;
  public const int CellSoldierHitpoints = 4;
  public const int CellDroneHitpoints = 1;
  public const int CellHolderHitpoints = 16;

  public static Dictionary<CellType, int> DroneCostByType = new Dictionary<CellType, int>()
  {
    { CellType.COLONY, CellColonyHitpoints },
    { CellType.BARRACKS, CellBarracksHitpoints },
    { CellType.HOLDER, CellHolderHitpoints }
  };

  public static Dictionary<CellType, Color> PlayerColors = new Dictionary<CellType, Color>()
  {
    { CellType.BASE, Color.green },
    { CellType.COLONY, Color.green },
    { CellType.DRONE, Color.green },
    { CellType.BARRACKS, Color.red },
    { CellType.HOLDER, Color.blue },
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
    SOLDIER,
    HOLDER,
    NONE
  }
}
