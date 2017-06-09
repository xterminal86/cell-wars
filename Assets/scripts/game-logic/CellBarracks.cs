using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for assault unit producers
/// </summary>
public class CellBarracks : CellBaseClass 
{  
  protected Dictionary<int, CellSoldier> _spawnedSoldiersById = new Dictionary<int, CellSoldier>();
  public Dictionary<int, CellSoldier> SpawnedSoldiersById
  {
    get { return _spawnedSoldiersById; }
  }

  protected int _soldiersLimit = 0;

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 1.0f;
    _animationSpeed = 0.1f;
  }
      
  protected int _spawnId = 0;
  protected bool CanSpawnCell(GlobalConstants.CellType cellToSpawn)
  {
    _spawnId = -1;

    // Check for vacant slots
    for (int i = 0; i < _soldiersLimit; i++)
    {
      if (_spawnedSoldiersById[i] == null)
      {
        _spawnId = i;
      }
    }

    return (LevelLoader.Instance.DronesCountByOwner[OwnerId] >= GlobalConstants.DroneCostByType[cellToSpawn] && _spawnId != -1);    
  }
}
