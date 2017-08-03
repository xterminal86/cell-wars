using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSpawner : CellBarracks
{  
  public CellSpawner()
  {
    Type = GlobalConstants.CellType.SPAWNER;
    Hitpoints = GlobalConstants.CellSpawnerHitpoints;
    Priority = GlobalConstants.CellSpawnerPriority;

    _soldiersLimit = GlobalConstants.SoldiersPerSpawner;

    for (int i = 0; i < _soldiersLimit; i++)
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
    base.Update();

    if (LevelLoader.Instance.IsGameOver)
    {
      return;
    }

    if (_timer > GlobalConstants.AttackerSpawnTimeout)
    {   
      _timer = 0.0f;

      if (CanSpawnCell(GlobalConstants.CellType.ATTACKER))
      {          
        var res = TryToFindEmptyCell();
        if (res != null)
        { 
          LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[GlobalConstants.CellType.ATTACKER], OwnerId);
          var c = LevelLoader.Instance.PlaceCell(res, GlobalConstants.CellType.ATTACKER, OwnerId);

          _spawnedSoldiersById[_spawnId] = (c as CellSoldier);

          (c as CellSoldier).SpawnID = _spawnId;
          (c as CellSoldier).BarracksRef = this;

          LevelLoader.Instance.SoldiersCountByOwner[OwnerId]++;
        }
      }
    }

    _timer += Time.smoothDeltaTime;
  }
}
