using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellArsenal : CellBaseClass 
{
  Dictionary<int, CellHeavy> _spawnedSoldiersById = new Dictionary<int, CellHeavy>();
  public Dictionary<int, CellHeavy> SpawnedSoldiersById
  {
    get { return _spawnedSoldiersById; }
  }

  public CellArsenal()
  {
    Type = GlobalConstants.CellType.ARSENAL;
    Hitpoints = GlobalConstants.CellArsenalHitpoints;
    Priority = GlobalConstants.CellArsenalPriority;

    for (int i = 0; i < GlobalConstants.SoldiersPerArsenal; i++)
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

    PlayAnimation();

    if (LevelLoader.Instance.IsGameOver)
    {
      return;
    }

    if (_timer > GlobalConstants.HeavySpawnTimeSeconds)
    {   
      _timer = 0.0f;

      if (CanSpawnSoldier())
      {          
        var res = TryToFindEmptyCell();
        if (res != null)
        {          
          LevelLoader.Instance.TransformDrones(GlobalConstants.CellHeavyHitpoints, OwnerId);
          var c = LevelLoader.Instance.PlaceCell(res, GlobalConstants.CellType.HEAVY, OwnerId);

          _spawnedSoldiersById[_spawnId] = (c as CellHeavy);

          (c as CellHeavy).SpawnID = _spawnId;
          (c as CellHeavy).BarracksRef = this;

          LevelLoader.Instance.SoldiersCountByOwner[OwnerId]++;
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

    for (int i = 0; i < GlobalConstants.SoldiersPerArsenal; i++)
    {
      if (_spawnedSoldiersById[i] == null)
      {
        _spawnId = i;
      }
    }

    return (LevelLoader.Instance.DronesCountByOwner[OwnerId] >= GlobalConstants.CellHeavyHitpoints && _spawnId != -1);    
  }
}
