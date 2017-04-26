using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellColony : CellBaseClass 
{
  public CellColony()
  {
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
        SpawnCell(GlobalConstants.CellType.DRONE);
      }

      _timer = 0.0f;
    }

    _timer += Time.smoothDeltaTime;
  }
}
