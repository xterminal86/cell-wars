﻿using System.Collections;
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
    ModelTransform.Rotate(Vector3.right, Time.smoothDeltaTime * 10.0f);
    ModelTransform.Rotate(Vector3.up, Time.smoothDeltaTime * 20.0f);
    ModelTransform.Rotate(Vector3.forward, Time.smoothDeltaTime * 5.0f);

    if (_timer > GlobalConstants.SoldierSpawnTimeSeconds)
    {
      _timer = 0.0f;

      _unitsLimit = LevelLoader.Instance.BarracksCountByOwner[OwnerId] * GlobalConstants.SoldiersPerBarrack;

      if (LevelLoader.Instance.DronesCountByOwner[OwnerId] >= GlobalConstants.CellSoldierHitpoints 
       && LevelLoader.Instance.SoldiersCountByOwner[OwnerId] < _unitsLimit)
      {        
        LevelLoader.Instance.TransformDrones(GlobalConstants.CellSoldierHitpoints, OwnerId);
        TryToSpawnCell(GlobalConstants.CellType.SOLDIER);
      }
    }

    _timer += Time.smoothDeltaTime;
  }
}
