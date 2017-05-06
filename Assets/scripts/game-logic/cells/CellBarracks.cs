using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner - spawns attackers.
/// </summary>
public class CellBarracks : CellBaseClass 
{  
  Dictionary<int, CellSoldier> _spawnedSoldiersById = new Dictionary<int, CellSoldier>();
  public Dictionary<int, CellSoldier> SpawnedSoldiersById
  {
    get { return _spawnedSoldiersById; }
  }

  public CellBarracks()
  {
    Type = GlobalConstants.CellType.BARRACKS;
    Hitpoints = GlobalConstants.CellBarracksHitpoints;

    for (int i = 0; i < GlobalConstants.SoldiersPerBarrack; i++)
    {
      _spawnedSoldiersById[i] = null;
    }
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

    if (_timer > GlobalConstants.SoldierSpawnTimeSeconds)
    {   
      _timer = 0.0f;

      if (CanSpawnSoldier())
      {          
        var res = TryToFindEmptyCell();
        if (res != null)
        {          
          LevelLoader.Instance.TransformDrones(GlobalConstants.CellSoldierHitpoints, OwnerId);
          var c = LevelLoader.Instance.PlaceCell(res, GlobalConstants.CellType.SOLDIER, OwnerId);

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
