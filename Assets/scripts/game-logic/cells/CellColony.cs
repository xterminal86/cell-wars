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
  }

  float _timer = 0.0f;
  public override void Update()
  {    
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
