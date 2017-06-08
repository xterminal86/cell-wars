using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Colony - spawns drones around itself.
/// </summary>
public class CellColony : CellBaseClass 
{
  public CellColony()
  {
    Type = GlobalConstants.CellType.COLONY;
    Hitpoints = GlobalConstants.CellColonyHitpoints;
    Priority = GlobalConstants.CellColonyPriority;
  }

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 1.0f;
    _animationSpeed = 0.1f;
  }

  public override void Update()
  {    
    base.Update();

    if (LevelLoader.Instance.IsGameOver)
    {
      return;
    }

    PlayAnimation();
    TryToSpawnDrone();
  }
}
