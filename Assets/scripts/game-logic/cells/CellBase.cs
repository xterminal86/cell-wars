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

  float _timer = 0.0f;
  public override void Update()
  {
    PlayAnimation();

    if (_timer > GlobalConstants.DroneSpawnTimeSeconds)
    {   
      var res = TryToFindEmptyCell();
      if (res != null)
      {
        LevelLoader.Instance.PlaceCell(res, GlobalConstants.CellType.DRONE, OwnerId);
      }

      _timer = 0.0f;
    }

    _timer += Time.smoothDeltaTime;
  }
}
