using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crude implementation of AI
/// </summary>
public class AI : MonoBehaviour 
{  
  // Limit of build actions performed by CPU
  public int MaxBuildActions = int.MaxValue;

  int _buildActionsDone = 0;

  float _actionCooldownTimer = 0.0f;
  void Update()
  {
    if (_actionCooldownTimer < GlobalConstants.CPUActionTimeout)
    {
      _actionCooldownTimer += Time.smoothDeltaTime;
      return;
    }

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
        var obj = LevelLoader.Instance.ObjectsMap[x, y];

        // FIXME: hardcoded AI id

        if (obj != null && obj.CellInstance.OwnerId == 1)
        {
          if (obj.CellInstance.Type == GlobalConstants.CellType.BARRACKS)
          {
            _barracksBuilt++;
          }
          else if (obj.CellInstance.Type == GlobalConstants.CellType.COLONY)
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
    for (int x = 0; x < LevelLoader.Instance.MapSize; x++)
    {
      for (int y = 0; y < LevelLoader.Instance.MapSize; y++)
      {
        _pos.Set(x, y);

        if (LevelLoader.Instance.CheckLocationToBuild(_pos, 1, 0) && TryToBuild(_pos))
        {
          _actionCooldownTimer = 0.0f;
          _buildActionsDone++;
          return;
        }
      }
    }
  }

  bool TryToBuild(Int2 posToBuild)
  {
    var buildingType = (_coloniesBuilt > _barracksBuilt) ? GlobalConstants.CellType.BARRACKS : GlobalConstants.CellType.COLONY;

    if (LevelLoader.Instance.DronesCountByOwner[1] >= GlobalConstants.DroneCostByType[buildingType])
    {      
      LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[buildingType], 1);
      LevelLoader.Instance.PlaceCell(posToBuild, buildingType, 1);
      return true;
    }    

    return false;
  }
}
