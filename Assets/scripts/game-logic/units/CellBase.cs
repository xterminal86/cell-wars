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
    base.Update();

    TryToSpawnDrone();   
  }

  float _timer = 0.0f;
  void TryToSpawnDrone()
  {
    var res = TryToFindEmptyCell();
    if (res != null)
    {
      _timer += Time.smoothDeltaTime;

      if (_timer > GlobalConstants.DroneSpawnTimeout)
      {   
        LevelLoader.Instance.PlaceCell(res, GlobalConstants.CellType.DRONE, OwnerId);
        _timer = 0.0f;
      }
    }
    else
    {
      _timer = 0.0f;
    }
  }
}
