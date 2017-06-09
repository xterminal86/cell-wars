using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellArsenal : CellBarracks 
{  
  public CellArsenal()
  {
    Type = GlobalConstants.CellType.ARSENAL;
    Hitpoints = GlobalConstants.CellArsenalHitpoints;
    Priority = GlobalConstants.CellArsenalPriority;

    _soldiersLimit = GlobalConstants.SoldiersPerArsenal;

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

    if (_timer > GlobalConstants.HeavySpawnTimeout)
    {   
      _timer = 0.0f;

      if (CanSpawnCell(GlobalConstants.CellType.HEAVY))
      {          
        var res = TryToFindEmptyCell();
        if (res != null)
        {          
          LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[GlobalConstants.CellType.HEAVY], OwnerId);
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
}
