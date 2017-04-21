using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBarracks : CellBaseClass 
{
  // FIXME: soldiers by player

  int _soldiersProduced = 0;

  public CellBarracks()
  {
    Hitpoints = GlobalConstants.CellBarracksHitpoints;
  }

  float _timer = 0.0f;
  public override void Update()
  {
    BehaviourRef.transform.Rotate(Vector3.right, Time.smoothDeltaTime * 5.0f);
    BehaviourRef.transform.Rotate(Vector3.up, Time.smoothDeltaTime * 10.0f);
    BehaviourRef.transform.Rotate(Vector3.down, Time.smoothDeltaTime * 2.5f);

    if (_timer > GlobalConstants.SoldierSpawnTimeSeconds)
    {
      _timer = 0.0f;

      if (_soldiersProduced < 1)
      {
        _soldiersProduced++;
        SpawnDrone(GlobalConstants.CellType.SOLDIER);
      }
    }

    _timer += Time.smoothDeltaTime;
  }
}
