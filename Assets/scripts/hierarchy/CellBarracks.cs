using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBarracks : CellBaseClass 
{  
  Dictionary<int, CellSoldier> _spawnedSoldiersById = new Dictionary<int, CellSoldier>();
  public Dictionary<int, CellSoldier> SpawnedSoldiersById
  {
    get { return _spawnedSoldiersById; }
  }

  public CellBarracks()
  {
    Hitpoints = GlobalConstants.CellBarracksHitpoints;

    for (int i = 0; i < GlobalConstants.SoldiersPerBarrack; i++)
    {
      _spawnedSoldiersById[i] = null;
    }
  }

  float _timer = 0.0f;
  public override void Update()
  {
    ModelTransform.Rotate(Vector3.right, Time.smoothDeltaTime * 10.0f);
    ModelTransform.Rotate(Vector3.up, Time.smoothDeltaTime * 20.0f);
    ModelTransform.Rotate(Vector3.forward, Time.smoothDeltaTime * 5.0f);

    if (_timer > GlobalConstants.SoldierSpawnTimeSeconds)
    {   
      _timer = 0.0f;

      if (CanSpawnSoldier())
      {          
        var res = TryToFindEmptyCell();
        if (res != null)
        {          
          LevelLoader.Instance.TransformDrones(GlobalConstants.CellSoldierHitpoints, OwnerId);
          var c = SpawnCell(GlobalConstants.CellType.SOLDIER);

          _spawnedSoldiersById[_spawnId] = (c as CellSoldier);

          (c as CellSoldier).SpawnID = _spawnId;
          (c as CellSoldier).BarracksRef = this;
        }
      }
    }

    _timer += Time.smoothDeltaTime;
  }
    
  int _spawnId = 0;
  bool CanSpawnSoldier()
  {
    _spawnId = -1;

    // Check for vacant slots

    for (int i = 0; i < GlobalConstants.SoldiersPerBarrack; i++)
    {
      if (_spawnedSoldiersById[i] == null)
      {
        _spawnId = i;
      }
    }

    return (LevelLoader.Instance.DronesCountByOwner[OwnerId] >= GlobalConstants.CellSoldierHitpoints && _spawnId != -1);    
  }
}
