using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour 
{
  LevelLoader _level;

  int _maxBuildActions = 3;
  int _buildActionsDone = 0;

  void Start()
  {
    _level = LevelLoader.Instance;
  }

  void Update()
  {
    if (_buildActionsDone < _maxBuildActions)
    {
      if (BuildDone())
      {
        _buildActionsDone++;
      }
    }
  }

  Int2 _pos = Int2.Zero;
  bool BuildDone()
  {
    for (int x = 0; x < _level.MapSize; x++)
    {
      for (int y = 0; y < _level.MapSize; y++)
      {
        _pos.Set(x, y);

        if (_level.CheckLocationToBuild(_pos, 1))
        {
          return TryToBuild(_pos);         
        }
      }
    }

    return false;
  }

  bool TryToBuild(Int2 posToBuild)
  {
    bool res = false;

    switch (_buildActionsDone)
    {
      case 0:
        if (_level.DronesCountByOwner[1] >= GlobalConstants.ColonyDronesCost)
        {
          _level.Build(posToBuild, GlobalConstants.CellType.COLONY, 1);
          res = true;
        }         
        break;

      case 1:
        if (_level.DronesCountByOwner[1] >= GlobalConstants.BarracksDronesCost)
        {
          _level.Build(posToBuild, GlobalConstants.CellType.BARRACKS, 1);
          res = true;
        }         
        break;
    }

    return res;
  }
}
