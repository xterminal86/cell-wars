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
  }

  float _timer = 0.0f;
  public override void Update()
  {
    if (_timer > GlobalConstants.DroneSpawnTimeSeconds)
    {   
      var res = TryToFindEmptyCell();

      if (res != null)
      {
        SpawnCell(GlobalConstants.CellType.DRONE);
      }

      _timer = 0.0f;
    }

    _timer += Time.smoothDeltaTime;
  }
}
