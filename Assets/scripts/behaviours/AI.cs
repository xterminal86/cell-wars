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

  bool _buildDone = false;
  float _actionCooldownTimer = 0.0f;
  void Update()
  {
    // To simulate pause between actions

    if (_buildDone)
    {
      if (_actionCooldownTimer > GlobalConstants.CPUActionTimeout)
      {
        _actionCooldownTimer = 0.0f;
        _buildDone = false;
        return;
      }

      _actionCooldownTimer += Time.smoothDeltaTime;
      return;
    }

    CalculateHeuristic();
    MakeDecision();
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

  // Type 1:
  // if (cpu_attackers <= player_attackers && cpu_colonies > cpu_barracks * 2) BuildBarracks();
  // if (player_barracks > cpu_defenders * 2) BuildDefender()
  // BuildHolderNearDefender()
  //
  // Type 2:
  // if (player_barracks > cpu_defenders * 2) BuildDefender()
  // BuildHolderNearDefender()
  // if (cpu_colonies > cpu_barracks * 2) BuildBarracks()
    
  void MakeDecision()
  {
    TryToBuildColony();
  }

  /// <summary>
  /// Looks for an empty cell with 8 empty cells around it with no drones overlapping.
  /// If there is no such location, try to build on any valid spot with maximum amount of
  /// empty cells around to maximize number of drones that can be spawned by this colony.
  /// </summary>
  void TryToBuildColony()
  {
    if (!GetCellsForBuilding(true))
      GetCellsForBuilding(false);
    
    int max = 0;
    int index = -1;
    Int2 posToBuild = Int2.Zero;

    // Looking for the best ranked location
    for (int i = 0; i < _rankedCells.Count; i++)
    {      
      if (_rankedCells[i].Value > max)
      {
        max = _rankedCells[i].Value;
        posToBuild.Set(_rankedCells[i].Key);
        index = i;
      }
    }

    if (index != -1)
    {
      if (TryToBuild(posToBuild, GlobalConstants.CellType.COLONY))
      {
        _buildDone = true;
      }
    }
  }

  List<KeyValuePair<Int2, int>> _rankedCells = new List<KeyValuePair<Int2, int>>();
  bool GetCellsForBuilding(bool optimal)
  {
    _rankedCells.Clear();

    for (int x = 0; x < LevelLoader.Instance.MapSize; x++)
    {
      for (int y = 0; y < LevelLoader.Instance.MapSize; y++)
      {  
        if (LevelLoader.Instance.CheckLocationToBuild(new Int2(x, y), 1, 0))
        {
          int rank = CalculateRank(new Int2(x, y));

          if (optimal)
          {            
            // Excluding cell too close to existing
            if (CheckForNearbyBuildings(new Int2(x, y)))
            {
              _rankedCells.Add(new KeyValuePair<Int2, int>(new Int2(x, y), rank));
            }
          }
          else
          {
            _rankedCells.Add(new KeyValuePair<Int2, int>(new Int2(x, y), rank));
          }
        }
      }
    }

    return (_rankedCells.Count != 0);
  }

  bool CheckForNearbyBuildings(Int2 cellCoords)
  {
    int lx = cellCoords.X - 2;
    int ly = cellCoords.Y - 2;
    int hx = cellCoords.X + 2;
    int hy = cellCoords.Y + 2;

    // Two vertical lines
    for (int x = lx; x <= hx; x++)
    {
      if (x >= 0 && x < LevelLoader.Instance.MapSize
       && ly >= 0 && hy < LevelLoader.Instance.MapSize)
      {
        if ((LevelLoader.Instance.ObjectsMap[x, ly] != null && LevelLoader.Instance.ObjectsMap[x, ly].CellInstance.Type != GlobalConstants.CellType.DRONE)
         || (LevelLoader.Instance.ObjectsMap[x, hy] != null && LevelLoader.Instance.ObjectsMap[x, hy].CellInstance.Type != GlobalConstants.CellType.DRONE))
        {
          return false;
        }
      }
    }

    // Two horizontal lines (excluding cells previously accounted for)
    for (int y = ly + 1; y <= hy - 1; y++)
    {
      if (y >= 0 && y < LevelLoader.Instance.MapSize
       && lx >= 0 && hx < LevelLoader.Instance.MapSize)
      {
        if ((LevelLoader.Instance.ObjectsMap[lx, y] != null && LevelLoader.Instance.ObjectsMap[lx, y].CellInstance.Type != GlobalConstants.CellType.DRONE)
         || (LevelLoader.Instance.ObjectsMap[hx, y] != null && LevelLoader.Instance.ObjectsMap[hx, y].CellInstance.Type != GlobalConstants.CellType.DRONE))
        {
          return false;
        }
      }
    }
      
    return true;
  }

  int CalculateRank(Int2 cellCoords)
  {
    int rank = 0;

    int lx = cellCoords.X - 1;
    int ly = cellCoords.Y - 1;
    int hx = cellCoords.X + 1;
    int hy = cellCoords.Y + 1;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
         && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          if (LevelLoader.Instance.ObjectsMap[x, y] == null)
          {
            rank++;
          }
        }
      }
    }

    return rank;
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

  bool TryToBuild(Int2 pos, GlobalConstants.CellType buildingType)
  {
    if (LevelLoader.Instance.DronesCountByOwner[1] >= GlobalConstants.DroneCostByType[buildingType])
    {
      LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[buildingType], 1);
      LevelLoader.Instance.PlaceCell(pos, buildingType, 1);
      return true;
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
