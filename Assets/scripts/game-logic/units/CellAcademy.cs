using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellAcademy : CellBarracks
{
  public CellAcademy()
  {
    Type = GlobalConstants.CellType.ACADEMY;
    Hitpoints = GlobalConstants.CellAcademyHitpoints;
    Priority = GlobalConstants.CellAcademyPriority;

    _soldiersLimit = GlobalConstants.SoldiersPerAcademy;

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

    if (_timer > GlobalConstants.SniperSpawnTimeout)
    {   
      _timer = 0.0f;

      if (CanSpawnCell(GlobalConstants.CellType.SNIPER))
      {          
        var res = TryToFindEmptyCell();
        if (res != null)
        {          
          LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[GlobalConstants.CellType.SNIPER], OwnerId);
          var c = LevelLoader.Instance.PlaceCell(res, GlobalConstants.CellType.SNIPER, OwnerId);

          _spawnedSoldiersById[_spawnId] = (c as CellSniper);

          (c as CellSniper).SpawnID = _spawnId;
          (c as CellSniper).BarracksRef = this;

          LevelLoader.Instance.SoldiersCountByOwner[OwnerId]++;
        }
      }
    }

    _timer += Time.smoothDeltaTime;
  }
}
