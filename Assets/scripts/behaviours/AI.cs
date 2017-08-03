using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crude implementation of AI
/// </summary>
public class AI : MonoBehaviour 
{ 
  ControlStateBase _state;

  ControlStateIdle _idleState;

  void Awake()
  {
    _idleState = new ControlStateIdle(this);

    ChangeState(_idleState);
  }

  Text _debugText;
  void Start()
  {
    _debugText = GameObject.Find("debug-text").GetComponent<Text>();
  }

  void ChangeState(ControlStateBase newState)
  {
    Debug.Log(string.Format("[{0}] => [{1}]", _state, newState));

    _state = newState;
  }

  void Update()
  {
    #if UNITY_EDITOR
    _debugText.text = _state.HeuristicProperty.ToString();
    #endif

    _state.Run();
  }

  /*
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

      if (_buildActionsQueue.Count != 0 && _buildActionsQueue.Peek().BuildingType != GlobalConstants.CellType.COLONY)
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
    if (!TryToPlanBarracks())
    {
      if (!TryToPlanDefender())
      {
        TryToPlanColony();
      }
    }
  }

  bool TryToPlanDefender()
  {
    if (_heuristic.EnemyBarracks != 0 && (_heuristic.EnemyBarracks * 2 > _heuristic.OurDefenders))
    {
      // Check for already built defender and build near it if true
      foreach (var c in _enemyBarracks)
      {        
        bool defenderPresent = false;

        var line = Utils.BresenhamLine(c, LevelLoader.Instance.BaseCoordinatesByOwner[1]);
        foreach (var p in line)
        {          
          if (((LevelLoader.Instance.ObjectsMap[p.X, p.Y] != null
            && LevelLoader.Instance.ObjectsMap[p.X, p.Y].CellInstance.Type == GlobalConstants.CellType.DEFENDER
            && LevelLoader.Instance.ObjectsMap[p.X, p.Y].CellInstance.OwnerId == 1) || IsDefenderPlanned(p))
            && FindSpotAroundDefender(p))
          { 
            defenderPresent = true;

            _buildActionsQueue.Enqueue(new BuildAction(_defenderCompactPosition, GlobalConstants.CellType.COLONY));
            _buildActionsQueue.Enqueue(new BuildAction(_defenderCompactPosition, GlobalConstants.CellType.DEFENDER));

            break;        
          }
        }

        if (!defenderPresent)
        {
          foreach (var p in line)
          {
            if (IsPositionAlreadyEnqueued(p))
            {
              continue;
            }

            if (LevelLoader.Instance.CheckLocationToBuild(p, 1, 0))
            { 
              _buildActionsQueue.Enqueue(new BuildAction(p, GlobalConstants.CellType.COLONY));
              _buildActionsQueue.Enqueue(new BuildAction(p, GlobalConstants.CellType.DEFENDER));
              break;
            }
          }
        }
      }
    }

    return (_buildActionsQueue.Count != 0);
  }

  bool IsDefenderPlanned(Int2 pos)
  {
    foreach (var action in _buildActionsQueue)
    {
      if (action.PosToBuild.X == pos.X 
       && action.PosToBuild.Y == pos.Y 
       && action.BuildingType == GlobalConstants.CellType.DEFENDER)
      {
        return true;
      }
    }

    return false;
  }

  Int2 _defenderCompactPosition = Int2.Zero;
  bool FindSpotAroundDefender(Int2 pos)
  {
    for (int x = pos.X - 1; x <= pos.X + 1; x++)
    {
      for (int y = pos.Y - 1; y <= pos.Y + 1; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
         && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          _defenderCompactPosition.Set(x, y);

          if (IsPositionAlreadyEnqueued(_defenderCompactPosition))
          {
            continue;
          }

          if (x != pos.X && y != pos.Y && LevelLoader.Instance.CheckLocationToBuild(_defenderCompactPosition, 1, 0))
          {            
            return true;
          }
        }
      }
    }

    return false;
  }

  bool IsPositionAlreadyEnqueued(Int2 pos)
  {
    foreach (var action in _buildActionsQueue)
    {
      if (action.PosToBuild.X == pos.X && action.PosToBuild.Y == pos.Y)
      {
        return true;
      }
    }

    return false;
  }

  bool TryToPlanBarracks()
  {
    if (_heuristic.OurColonies > _heuristic.OurBarracks * 2
        && _heuristic.OurDrones > 8)
    {
      if (!GetCellsForColonyBuilding(false))
      {      
        return false;
      }

      Int2 coords = FindBestRankedCell();

      if (coords != null)
      {
        _buildActionsQueue.Enqueue(new BuildAction(coords, GlobalConstants.CellType.COLONY));
        _buildActionsQueue.Enqueue(new BuildAction(coords, GlobalConstants.CellType.SPAWNER));
      }
    }

    return (_buildActionsQueue.Count != 0);
  }

  /// <summary>
  /// Looks for an empty cell with 8 empty cells around it with no drones overlapping.
  /// If there is no such location, try to build on any valid spot with maximum amount of
  /// empty cells around to maximize number of drones that can be spawned by this colony.
  /// </summary>
  bool TryToPlanColony()
  {
    if (!GetCellsForColonyBuilding(true))
      GetCellsForColonyBuilding(false);

    Int2 coords = FindBestRankedCell();

    if (coords != null)
    {      
      _buildActionsQueue.Enqueue(new BuildAction(coords, GlobalConstants.CellType.COLONY));
    }

    return (_buildActionsQueue.Count != 0);
  }

  Int2 FindBestRankedCell()
  {
    int max = 0;
    int index = -1;
    Int2 posToBuild = Int2.Zero;

    for (int i = 0; i < _rankedCells.Count; i++)
    {      
      if (_rankedCells[i].Value > max)
      {
        max = _rankedCells[i].Value;
        posToBuild.Set(_rankedCells[i].Key);
        index = i;
      }
    }

    return ((index != -1) ? posToBuild : null);
  }

  List<KeyValuePair<Int2, int>> _rankedCells = new List<KeyValuePair<Int2, int>>();
  bool GetCellsForColonyBuilding(bool optimal)
  {
    Int2 pos = Int2.Zero;

    _rankedCells.Clear();

    for (int x = 0; x < LevelLoader.Instance.MapSize; x++)
    {
      for (int y = 0; y < LevelLoader.Instance.MapSize; y++)
      {  
        pos.Set(x, y);

        if (!LevelLoader.Instance.CheckLocationToBuild(pos, 1, 0))
        {
          continue;
        }

        if (optimal)
        {
          if (!IsAnotherColonyNearby(pos))
          {
            int rank = CalculateRank(pos);

            if (rank == 9)
            {
              _rankedCells.Add(new KeyValuePair<Int2, int>(new Int2(pos), rank));
            }
          }
        }
        else
        {
          int rank = CalculateRank(pos);

          _rankedCells.Add(new KeyValuePair<Int2, int>(new Int2(pos), rank));
        }
      }
    }

    return (_rankedCells.Count != 0);
  }

  bool IsAnotherColonyNearby(Int2 pos)
  {
    int lx = pos.X - 2;
    int ly = pos.Y - 2;
    int hx = pos.X + 2;
    int hy = pos.Y + 2;

    if (lx >= 0 && hx < LevelLoader.Instance.MapSize
     && ly >= 0 && hy < LevelLoader.Instance.MapSize)
    {
      for (int x = lx; x <= hx; x++)
      { 
        if ((LevelLoader.Instance.ObjectsMap[x, ly] != null && LevelLoader.Instance.ObjectsMap[x, ly].CellInstance.Type == GlobalConstants.CellType.COLONY)
         || (LevelLoader.Instance.ObjectsMap[x, hy] != null && LevelLoader.Instance.ObjectsMap[x, hy].CellInstance.Type == GlobalConstants.CellType.COLONY))
        {
          return true;
        }
      }

      for (int y = ly + 1; y <= hy - 1; y++)
      {
        if ((LevelLoader.Instance.ObjectsMap[lx, y] != null && LevelLoader.Instance.ObjectsMap[lx, y].CellInstance.Type == GlobalConstants.CellType.COLONY)
          || (LevelLoader.Instance.ObjectsMap[hx, y] != null && LevelLoader.Instance.ObjectsMap[hx, y].CellInstance.Type == GlobalConstants.CellType.COLONY))
        {
          return true;
        }
      }
    }

    return false;
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

      case GlobalConstants.CellType.SPAWNER:
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
  */
};


