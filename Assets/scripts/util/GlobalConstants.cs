using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants 
{
  // Time between new cell is spawned by building
  public const float DroneSpawnTimeSeconds = 1.0f;
  public const float SoldierSpawnTimeSeconds = 4.0f;

  // If there is no base or colony in range, drone dies after this number of seconds passed
  public const float AbandonedDroneLifetimeSeconds = 3.0f;

  // Attacker cooldown in seconds
  public const float SoldierAttackTimeout = 1.5f;
  public const float DefenderAttackTimeout = 2.0f;

  // Various game parameters
  public const float DefaultBulletSpeed = 2.0f;
  public const float DefenderBulletSpeed = 8.0f;
  public const float AttackerMoveSpeed = 0.5f;
  public const float HolderSlowFactor = 0.5f;
  public const float CameraZoomSpeed = 1.0f;
  public const float CameraMoveSpeed = 10.0f;
  public const float CellHolderRange = 3.0f;
  public const float CellDefenderRange = 4.0f;
  public const int RoundTimeSeconds = 120;

  // Cooldown between build actions for CPU to give player some advantage
  public const float CPUActionTimeout = 6.0f;

  public const int SoldiersPerBarrack = 1;

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
  public const int CellDefenderHitpoints = 8;

  public static Dictionary<CellType, int> CellHitpointsByType = new Dictionary<CellType, int>() 
  {
    { CellType.BASE, CellBaseHitpoints },
    { CellType.COLONY, CellColonyHitpoints },
    { CellType.BARRACKS, CellBarracksHitpoints },
    { CellType.SOLDIER, CellSoldierHitpoints },
    { CellType.DRONE, CellDroneHitpoints },
    { CellType.HOLDER, CellHolderHitpoints },
    { CellType.DEFENDER, CellDefenderHitpoints }
  };

  public static Dictionary<CellType, int> DroneCostByType = new Dictionary<CellType, int>()
  {
    { CellType.COLONY, CellColonyHitpoints },
    { CellType.BARRACKS, CellBarracksHitpoints },
    { CellType.HOLDER, CellHolderHitpoints },
    { CellType.DEFENDER, CellDefenderHitpoints }
  };

  public static Dictionary<CellType, Color> PlayerColors = new Dictionary<CellType, Color>()
  {
    { CellType.BASE, Color.green },
    { CellType.COLONY, Color.green },
    { CellType.DRONE, Color.green },
    { CellType.BARRACKS, Color.red },
    { CellType.HOLDER, Color.blue },
    { CellType.DEFENDER, Color.red },
    { CellType.SOLDIER, Color.red }
  };

  public static Dictionary<CellType, Color> CPUColors = new Dictionary<CellType, Color>()
  {
    { CellType.BASE, Color.cyan },
    { CellType.COLONY, Color.cyan },
    { CellType.DRONE, Color.cyan },
    { CellType.BARRACKS, Color.magenta },
    { CellType.HOLDER, Color.yellow },
    { CellType.DEFENDER, Color.magenta },
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
    DEFENDER,
    NONE
  }
}
