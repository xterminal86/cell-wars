using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour 
{
  LevelLoader _level;

  // Limit of build actions performed by CPU
  public int MaxBuildActions = int.MaxValue;

  int _buildActionsDone = 0;

  void Start()
  {
    _level = LevelLoader.Instance;
  }

  void Update()
  {
    CountBuildings();

    if (_buildActionsDone < MaxBuildActions)
    {
      DecideWhatToBuild();
    }
  }

  int _coloniesBuilt = 0, _barracksBuilt = 0;
  void CountBuildings()
  {
    _coloniesBuilt = 0;
    _barracksBuilt = 0;

    for (int x = 0; x < LevelLoader.Instance.MapSize; x++)
    {
      for (int y = 0; y < LevelLoader.Instance.MapSize; y++)
      {
        var cell = LevelLoader.Instance.Map[x, y];

        // FIXME: hardcoded AI id

        if (cell.CellHere != null && cell.CellHere.OwnerId == 1)
        {
          if (cell.CellHere.Type == GlobalConstants.CellType.BARRACKS)
          {
            _barracksBuilt++;
          }
          else if (cell.CellHere.Type == GlobalConstants.CellType.COLONY)
          {
            _coloniesBuilt++;
          }
        }
      }
    }
  }

  Int2 _pos = Int2.Zero;
  void DecideWhatToBuild()
  {
    for (int x = 0; x < _level.MapSize; x++)
    {
      for (int y = 0; y < _level.MapSize; y++)
      {
        _pos.Set(x, y);

        if (_level.CheckLocationToBuild(_pos, 1) && TryToBuild(_pos))
        {
          _buildActionsDone++;
        }
      }
    }
  }

  bool TryToBuild(Int2 posToBuild)
  {
    var buildingType = (_coloniesBuilt > _barracksBuilt) ? GlobalConstants.CellType.BARRACKS : GlobalConstants.CellType.COLONY;

    if (_level.DronesCountByOwner[1] >= GlobalConstants.DroneCostByType[buildingType])
    {      
      _level.Build(posToBuild, buildingType, 1);
      return true;
    }    

    return false;
  }
}
