using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBarracks : CellBaseClass 
{  
  public CellBarracks()
  {
    Hitpoints = GlobalConstants.CellBarracksHitpoints;
  }

  int _unitsLimit = 0;

  float _timer = 0.0f;
  public override void Update()
  {
    BehaviourRef.transform.Rotate(Vector3.right, Time.smoothDeltaTime * 5.0f);
    BehaviourRef.transform.Rotate(Vector3.up, Time.smoothDeltaTime * 10.0f);
    BehaviourRef.transform.Rotate(Vector3.down, Time.smoothDeltaTime * 2.5f);

    if (_timer > GlobalConstants.SoldierSpawnTimeSeconds)
    {
      _timer = 0.0f;

      _unitsLimit = LevelLoader.Instance.BarracksCountByOwner[OwnerId] * GlobalConstants.SoldiersPerBarrack;

      if (LevelLoader.Instance.DronesCountByOwner[OwnerId] >= GlobalConstants.CellSoldierHitpoints 
       && LevelLoader.Instance.SoldiersCountByOwner[OwnerId] < _unitsLimit)
      {        
        LevelLoader.Instance.TransformDrones(GlobalConstants.CellSoldierHitpoints, OwnerId);
        SpawnCell(GlobalConstants.CellType.SOLDIER);
      }
    }

    _timer += Time.smoothDeltaTime;
  }
}
