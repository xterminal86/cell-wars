using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBase : CellBaseClass
{
  public CellBase()
  {
    Hitpoints = GlobalConstants.CellBaseHitpoints;
  }

  float _timer = 0.0f;
  public override void Update()
  {
    if (_timer > GlobalConstants.DroneSpawnTimeSeconds)
    {
      _timer = 0.0f;
      SpawnCell(GlobalConstants.CellType.DRONE);
    }

    _timer += Time.smoothDeltaTime;
  }
}
