using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main building of a player, works like colony. 
/// If it's destroyed - it's game over.
/// </summary>
public class CellBase : CellBaseClass
{
  public CellBase()
  {
    Type = GlobalConstants.CellType.BASE;
    Hitpoints = GlobalConstants.CellBaseHitpoints;
    Priority = GlobalConstants.CellBasePriority;
  }

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 1.0f;
    _animationSpeed = 0.1f;
  }

  public override void Update()
  {
    PlayAnimation();
    TryToSpawnDrone();   
  }
}
