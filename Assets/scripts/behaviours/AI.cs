using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crude implementation of AI
/// </summary>
public class AI : MonoBehaviour 
{  
  Heuristic _heuristic = new Heuristic();

  // Limit of build actions performed by CPU
  public int MaxBuildActions = int.MaxValue;

  int _buildActionsDone = 0;

  Text _debugText;
  void Start()
  {
    _debugText = GameObject.Find("debug-text").GetComponent<Text>();
  }

  float _actionCooldownTimer = 0.0f;
  void Update()
  {
    CalculateHeuristic();

    // To simulate pause between actions

    if (_buildDone)
    {
      if (_actionCooldownTimer > GlobalConstants.CPUActionTimeout)
      {
        _buildDone = false;
        return;
      }

      _actionCooldownTimer += Time.smoothDeltaTime;
      return;
    }

    CountBuildings();

    if (_buildActionsDone < MaxBuildActions)
    {      
      DecideWhatToBuild();
    }
  }

  void CalculateHeuristic()
  {
    _heuristic.Clear();

    _heuristic.EnemyArea = LevelLoader.Instance.TerritoryCountByOwner[0];
    _heuristic.OurArea = LevelLoader.Instance.TerritoryCountByOwner[1];

    for (int x = 0; x < LevelLoader.Instance.MapSize; x++)
    {
      for (int y = 0; y < LevelLoader.Instance.MapSize; y++)
      {
        var cellObject = LevelLoader.Instance.ObjectsMap[x, y];

        if (cellObject != null)
        {
          FillHeuristic(cellObject.CellInstance);
        }
      }
    }

    _heuristic.OurScore = LevelLoader.Instance.ScoreCountByOwner[1];
    _heuristic.EnemyScore = LevelLoader.Instance.ScoreCountByOwner[0];

    _heuristic.EnemyAttackers = LevelLoader.Instance.SoldiersCountByOwner[0];
    _heuristic.OurAttackers = LevelLoader.Instance.SoldiersCountByOwner[1];

    #if UNITY_EDITOR
    _debugText.text = _heuristic.ToString();
    #endif
  }

  void FillHeuristic(CellBaseClass cellObject)
  {
    switch (cellObject.Type)
    {
      case GlobalConstants.CellType.COLONY:
        if (cellObject.OwnerId == 0)
        {
          _heuristic.EnemyColonies++;
        }
        else
        {
          _heuristic.OurColonies++;
        }
        break;      

      case GlobalConstants.CellType.BARRACKS:
        if (cellObject.OwnerId == 0)
        {
          _heuristic.EnemyBarracks++;
        }
        else
        {
          _heuristic.OurBarracks++;
        }
        break;

      case GlobalConstants.CellType.DRONE:
        if (cellObject.OwnerId == 0)
        {
          _heuristic.EnemyDrones++;
        }
        else
        {
          _heuristic.OurDrones++;
        }
        break;      
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

  bool _buildDone = false;
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
          _buildDone = true;
          _actionCooldownTimer = 0.0f;
          _buildActionsDone++;
          return;
        }
      }
    }
  }

  bool TryToBuild(Int2 posToBuild)
  {
    if (_coloniesBuilt >= 2 && _coloniesBuilt >= _barracksBuilt * 2)
    {
      foreach (var item in LevelLoader.Instance.BuildingsCoordinatesByOwner[1])
      {
        var obj = LevelLoader.Instance.ObjectsMap[item.X, item.Y];

        if (obj != null && obj.CellInstance.Type == GlobalConstants.CellType.COLONY)
        {
          if (LevelLoader.Instance.DronesCountByOwner[1] >= GlobalConstants.DroneCostByType[GlobalConstants.CellType.BARRACKS])
          {            
            posToBuild.Set(obj.CellInstance.Coordinates);
            LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[GlobalConstants.CellType.BARRACKS], 1);
            obj.DestroySelf();
            LevelLoader.Instance.PlaceCell(posToBuild, GlobalConstants.CellType.BARRACKS, 1);
            return true;
          }
        }
      }
    }
    else
    {
      if (LevelLoader.Instance.DronesCountByOwner[1] >= GlobalConstants.DroneCostByType[GlobalConstants.CellType.COLONY])
      {      
        LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[GlobalConstants.CellType.COLONY], 1);
        LevelLoader.Instance.PlaceCell(posToBuild, GlobalConstants.CellType.COLONY, 1);
        return true;
      }    
    }

    return false;
  }
}

public class Heuristic
{
  public int OurColonies = 0;
  public int EnemyColonies = 0;
  public int OurBarracks = 0;
  public int EnemyBarracks = 0;
  public int OurDrones = 0;
  public int EnemyDrones = 0;
  public int OurArea = 0;
  public int EnemyArea = 0;
  public int OurAttackers = 0;
  public int EnemyAttackers = 0;
  public int OurScore = 0;
  public int EnemyScore = 0;

  public void Clear()
  {
    OurColonies = 0;
    EnemyColonies = 0;
    OurBarracks = 0;
    EnemyBarracks = 0;
    OurDrones = 0;
    EnemyDrones = 0;
    OurArea = 0;
    EnemyArea = 0;
    OurAttackers = 0;
    EnemyAttackers = 0;
    OurScore = 0;
    EnemyScore = 0;
  }

  public override string ToString()
  {
    return string.Format("CPU colonies:     {0}\n" +
                         "Player colonies:  {1}\n" + 
                         "CPU barracks:     {2}\n" +
                         "Player barracks:  {3}\n" +
                         "CPU drones:       {4}\n" +
                         "Player drones:    {5}\n" +
                         "CPU territory:    {6}\n" +
                         "Player territory: {7}\n" +
                         "CPU attackers:    {8}\n" +
                         "Player attackers: {9}\n" +
                         "CPU score:        {10}\n" +
                         "Player score:     {11}\n", 
      OurColonies, EnemyColonies, OurBarracks, EnemyBarracks, OurDrones, EnemyDrones, OurArea, EnemyArea, OurAttackers, EnemyAttackers, OurScore, EnemyScore);
  }
};
