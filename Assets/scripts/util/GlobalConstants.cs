using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants 
{
  // Time between new cell is spawned by building
  public const float DroneSpawnTimeout = 0.5f; //1.5f;
  public const float AttackerSpawnTimeout = 4.0f;
  public const float HeavySpawnTimeout = 10.0f;
  public const float SniperSpawnTimeout = 5.0f;

  // Attacker cooldown in seconds
  public const float AttackerAttackTimeout = 1.0f;
  public const float SniperAttackTimeout = 4.0f;
  public const float HeavyAttackTimeout = 10.0f;
  public const float DefenderAttackTimeout = 2.0f;

  // Various game parameters
  public const float DefaultBulletSpeed = 1.0f;
  public const float DefenderBulletSpeed = 2.0f;
  public const float SniperBulletSpeed = 4.0f;
  public const float AttackerMoveSpeed = 0.5f;
  public const float SniperMoveSpeed = 1.0f;
  public const float HeavyMoveSpeed = 0.2f;
  public const float HolderSlowFactor = 0.5f;
  public const float CameraZoomSpeed = 1.0f;
  public const float CameraMoveSpeed = 10.0f;
  public const float CellHolderRange = 3.0f;
  public const float CellDefenderRange = 4.0f;
  public const float CellAttackerRange = 1.5f;
  public const float CellSniperRange = 3.0f;
  public const float CellHeavyMinRange = 3.0f;
  public const float CellHeavyMaxRange = 5.0f;
  public const float CellHeavySplashRadius = 2.5f;
  public const int RoundTimeSeconds = 300;

  // Attacker priorities for targets
  public const int CellWallPriority = 1;
  public const int CellDronePriority = 2;
  public const int CellColonyPriority = 100;
  public const int CellAttackerPriority = 110;
  public const int CellSniperPriority = 150;
  public const int CellHeavyPriority = 160;
  public const int CellDefenderPriority = 60;
  public const int CellSpawnerPriority = 60;
  public const int CellAcademyPriority = 60;
  public const int CellArsenalPriority = 70;
  public const int CellHolderPriority = 50;
  public const int CellBasePriority = 200;

  // Cooldown between build actions for CPU to give player some advantage
  public const float CPUActionTimeout = 3.0f;
  // CPU timeout for building from colony
  public const float CPUIntermediateActionTimeout = 2.0f;

  public const int SoldiersPerSpawner = 2;
  public const int SoldiersPerAcademy = 1;
  public const int SoldiersPerArsenal = 1;

  public const int CellHeavyDamage = 8;
  public const int CellSniperDamage = 4;

  // Maximum block distance between already built building 
  // and new spot for a new building.
  public const int BuildRangeDistance = 3;
  // Maximum allowed distance between players' buildings.
  public const int DMZRange = 8;

  // Various cells hitpoints
  public const int CellBaseHitpoints = 32;
  public const int CellColonyHitpoints = 8;
  public const int CellSpawnerHitpoints = 8;
  public const int CellAcademyHitpoints = 8;
  public const int CellArsenalHitpoints = 16;
  public const int CellAttackerHitpoints = 4;
  public const int CellSniperHitpoints = 1;
  public const int CellHeavyHitpoints = 8;
  public const int CellDroneHitpoints = 1;
  public const int CellHolderHitpoints = 16;
  public const int CellDefenderHitpoints = 8;
  public const int CellWallHitpoints = 32;

  public static Dictionary<CellType, int> CellHitpointsByType = new Dictionary<CellType, int>() 
  {
    { CellType.BASE, CellBaseHitpoints },
    { CellType.COLONY, CellColonyHitpoints },
    { CellType.SPAWNER, CellSpawnerHitpoints },
    { CellType.ACADEMY, CellAcademyHitpoints },
    { CellType.ARSENAL, CellArsenalHitpoints },
    { CellType.ATTACKER, CellAttackerHitpoints },
    { CellType.SNIPER, CellSniperHitpoints },
    { CellType.HEAVY, CellHeavyHitpoints },
    { CellType.DRONE, CellDroneHitpoints },
    { CellType.HOLDER, CellHolderHitpoints },
    { CellType.DEFENDER, CellDefenderHitpoints },
    { CellType.WALL, CellWallHitpoints }
  };

  public static Dictionary<CellType, int> DroneCostByType = new Dictionary<CellType, int>()
  {
    { CellType.COLONY, CellColonyHitpoints },
    { CellType.SPAWNER, CellSpawnerHitpoints },
    { CellType.ACADEMY, CellAcademyHitpoints },
    { CellType.ARSENAL, CellArsenalHitpoints },
    { CellType.HOLDER, CellHolderHitpoints },
    { CellType.DEFENDER, CellDefenderHitpoints },
    { CellType.ATTACKER, CellAttackerHitpoints },
    { CellType.SNIPER, CellHeavyHitpoints },
    { CellType.HEAVY, CellHeavyHitpoints }
  };

  public static Dictionary<CellType, Color> PlayerColors = new Dictionary<CellType, Color>()
  {
    { CellType.BASE, Color.green },
    { CellType.COLONY, Color.green },
    { CellType.DRONE, Color.green },
    { CellType.SPAWNER, Color.red },
    { CellType.ACADEMY, Color.red },
    { CellType.ARSENAL, Color.red },
    { CellType.HOLDER, Color.blue },
    { CellType.DEFENDER, Color.red },
    { CellType.ATTACKER, Color.red },
    { CellType.SNIPER, Color.red },
    { CellType.HEAVY, Color.red }
  };

  public static Dictionary<CellType, Color> CPUColors = new Dictionary<CellType, Color>()
  {
    { CellType.BASE, Color.cyan },
    { CellType.COLONY, Color.cyan },
    { CellType.DRONE, Color.cyan },
    { CellType.SPAWNER, Color.magenta },
    { CellType.ACADEMY, Color.magenta },
    { CellType.ARSENAL, Color.magenta },
    { CellType.HOLDER, Color.yellow },
    { CellType.DEFENDER, Color.magenta },
    { CellType.ATTACKER, Color.magenta },
    { CellType.SNIPER, Color.magenta },
    { CellType.HEAVY, Color.magenta }
  };

  public static List<Dictionary<CellType, Color>> ColorsList = new List<Dictionary<CellType, Color>>()
  {
    PlayerColors, CPUColors
  };

  // Loop start and end in samples of a track by name
  public static Dictionary<string, Int2> MusicTrackByName = new Dictionary<string, Int2>()
  {
    { "in-game-1", new Int2(1209531, 8769285) }
  };

  public enum CellType
  {    
    BASE = 0,
    COLONY,
    DRONE,
    SPAWNER,
    ACADEMY,
    ARSENAL,
    ATTACKER,
    SNIPER,
    HEAVY,
    HOLDER,
    DEFENDER,
    WALL,
    NONE
  }
}
