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

  Queue<BuildAction> _buildActionsQueue = new Queue<BuildAction>();

  Text _debugText;
  Text _buildActionsText;

  void Start()
  {
    _debugText = GameObject.Find("debug-text").GetComponent<Text>();
    _buildActionsText = GameObject.Find("build-actions-text").GetComponent<Text>();
  }

  bool _buildDone = false;
  bool _intermediateBuildDone = false;
  float _actionCooldownTimer = 0.0f;
  string _buildActionsString = string.Empty;
  void Update()
  {    
    #if UNITY_EDITOR
    _debugText.text = _heuristic.ToString();

    _buildActionsString = "";

    int counter = 1;
    foreach (var a in _buildActionsQueue)
    {
      _buildActionsString += string.Format("{0}: {1} {2}\n", counter, a.PosToBuild, a.BuildingType);
      counter++;
    }

    _buildActionsText.text = _buildActionsString;
    #endif

    if (_buildActionsDone == MaxBuildActions)
    {
      return;
    }

    // To simulate pause between actions

    if (_intermediateBuildDone)
    {
      if (_actionCooldownTimer > GlobalConstants.CPUIntermediateActionTimeout)
      {
        _actionCooldownTimer = 0.0f;
        _intermediateBuildDone = false;
        return;
      }

      var action = _buildActionsQueue.Peek();

      // If our intermediate build position invalidated during timeout (colony was destroyed or was occupied by enemy),
      // discard it
      if (action.BuildingType != GlobalConstants.CellType.COLONY 
       && (LevelLoader.Instance.ObjectsMap[action.PosToBuild.X, action.PosToBuild.Y] == null
       || (LevelLoader.Instance.ObjectsMap[action.PosToBuild.X, action.PosToBuild.Y] != null
        && LevelLoader.Instance.ObjectsMap[action.PosToBuild.X, action.PosToBuild.Y].CellInstance.OwnerId == 0)))
      {        
        _buildActionsQueue.Clear();
        _buildDone = true;
        _intermediateBuildDone = false;
        _actionCooldownTimer = 0.0f;
        return;
      }

      _actionCooldownTimer += Time.smoothDeltaTime;
      return;
    }

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

    if (_buildActionsQueue.Count != 0)
    {
      ProcessBuildActionsQueue();
    }
    else
    {
      MakeDecision();
    }
  }

  List<Int2> _enemyBarracks = new List<Int2>();
  void CalculateHeuristic()
  {
    _heuristic.Clear();

    _heuristic.EnemyArea = LevelLoader.Instance.TerritoryCountByOwner[0];
    _heuristic.OurArea = LevelLoader.Instance.TerritoryCountByOwner[1];

    _enemyBarracks.Clear();

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
  }

  void ProcessBuildActionsQueue()
  {   
    var action = _buildActionsQueue.Peek();

    if (TryToBuild(action.PosToBuild, action.BuildingType))
    {  
      _buildActionsDone++;

      _buildActionsQueue.Dequeue();

      if (_buildActionsQueue.Count != 0)
      {
        _intermediateBuildDone = true;
      }
      else
      {
        _buildDone = true;
      }
    }
  }

  // Type 1:
  // if (cpu_attackers <= player_attackers && cpu_colonies > cpu_barracks * 2) BuildBarracks();
  // if (player_barracks * 2 > cpu_defenders) BuildDefender()
  // BuildHolderNearDefender()
  //
  // Type 2:
  // if (player_barracks > cpu_defenders * 2) BuildDefender()
  // BuildHolderNearDefender()
  // if (cpu_colonies > cpu_barracks * 2) BuildBarracks()

  void MakeDecision()
  {
    ExecuteStrategy1();
  }

  void ExecuteStrategy1()
  {
    if (_heuristic.OurColonies > _heuristic.OurBarracks * 2)
    {
      TryToBuildBarracks();
    }
    else if (_heuristic.EnemyBarracks != 0 && (_heuristic.EnemyBarracks * 2 >= _heuristic.OurDefenders))
    {
      TryToBuildDefender();
    }
    else
    {
      TryToBuildColony();
    }
  }

  void TryToBuildDefender()
  {
    foreach (var c in _enemyBarracks)
    {
      var line = Utils.BresenhamLine(c, LevelLoader.Instance.BaseCoordinatesByOwner[1]);
      foreach (var p in line)
      {
        if (LevelLoader.Instance.CheckLocationToBuild(p, 1, 0))
        {  
          // If we have, for example, 6 barracks and only one valid spot for
          // defender, it will enqueue same spot 6 times. To prevent this check if
          // given point is already in the queue.

          bool alreadyAdded = false;
          foreach (var action in _buildActionsQueue)
          {
            if (action.PosToBuild.X == p.X && action.PosToBuild.Y == p.Y)
            {
              alreadyAdded = true;
              break;
            }
          }

          if (alreadyAdded)
          {
            continue;
          }
          
          _buildActionsQueue.Enqueue(new BuildAction(p, GlobalConstants.CellType.COLONY));
          _buildActionsQueue.Enqueue(new BuildAction(p, GlobalConstants.CellType.DEFENDER));
          break;
        }
      }
    }
  }

  void TryToBuildBarracks()
  {
    if (!GetCellsForColonyBuilding(false))
    {      
      return;
    }

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
      _buildActionsQueue.Enqueue(new BuildAction(posToBuild, GlobalConstants.CellType.COLONY));
      _buildActionsQueue.Enqueue(new BuildAction(posToBuild, GlobalConstants.CellType.BARRACKS));
    }
  }

  /// <summary>
  /// Looks for an empty cell with 8 empty cells around it with no drones overlapping.
  /// If there is no such location, try to build on any valid spot with maximum amount of
  /// empty cells around to maximize number of drones that can be spawned by this colony.
  /// </summary>
  void TryToBuildColony()
  {
    if (!GetCellsForColonyBuilding(true))
      GetCellsForColonyBuilding(false);
    
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
      _buildActionsQueue.Enqueue(new BuildAction(posToBuild, GlobalConstants.CellType.COLONY));
    }
  }

  List<KeyValuePair<Int2, int>> _rankedCells = new List<KeyValuePair<Int2, int>>();
  bool GetCellsForColonyBuilding(bool optimal)
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

  List<KeyValuePair<Int2, int>> _cellsByDistanceFromBase = new List<KeyValuePair<Int2, int>>();
  void FindSpotForDefender()
  {
    Int2 pos = Int2.Zero;
      
    _cellsByDistanceFromBase.Clear();
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
          _enemyBarracks.Add(cellObject.Coordinates);
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

      case GlobalConstants.CellType.DEFENDER:
        if (cellObject.OwnerId == 0)
        {
          _heuristic.EnemyDefenders++;
        }
        else
        {
          _heuristic.OurDefenders++;
        }
        break;
    }
  }

  bool TryToBuild(Int2 pos, GlobalConstants.CellType buildingType)
  {
    if (LevelLoader.Instance.DronesCountByOwner[1] >= GlobalConstants.DroneCostByType[buildingType])
    {
      // It can only be a colony
      if (LevelLoader.Instance.ObjectsMap[pos.X, pos.Y] != null)
      {
        LevelLoader.Instance.ObjectsMap[pos.X, pos.Y].DestroySelf();
      }

      LevelLoader.Instance.TransformDrones(GlobalConstants.DroneCostByType[buildingType], 1);
      LevelLoader.Instance.PlaceCell(pos, buildingType, 1);
      return true;
    }

    return false;
  }
}

public class BuildAction
{
  public Int2 PosToBuild = Int2.Zero;
  public GlobalConstants.CellType BuildingType = GlobalConstants.CellType.NONE;

  public BuildAction(Int2 posToBuild, GlobalConstants.CellType buildingType)
  {
    PosToBuild.Set(posToBuild);
    BuildingType = buildingType;
  }
};

public class Heuristic
{
  public int OurColonies = 0;
  public int EnemyColonies = 0;
  public int OurBarracks = 0;
  public int EnemyBarracks = 0;
  public int OurDefenders = 0;
  public int EnemyDefenders = 0;
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
    OurDefenders = 0;
    EnemyDefenders = 0;
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
    return string.Format("AI data:\n" +
                         "CPU colonies:     {0}\n" +
                         "Player colonies:  {1}\n" + 
                         "CPU barracks:     {2}\n" +
                         "Player barracks:  {3}\n" +
                         "CPU defenders:    {4}\n" +
                         "Player defenders: {5}\n" +
                         "CPU drones:       {6}\n" +
                         "Player drones:    {7}\n" +
                         "CPU territory:    {8}\n" +
                         "Player territory: {9}\n" +
                         "CPU attackers:    {10}\n" +
                         "Player attackers: {11}\n" +
                         "CPU score:        {12}\n" +
                         "Player score:     {13}\n", 
      OurColonies, EnemyColonies, OurBarracks, EnemyBarracks, OurDefenders, EnemyDefenders, OurDrones, EnemyDrones, OurArea, EnemyArea, OurAttackers, EnemyAttackers, OurScore, EnemyScore);
  }
};
