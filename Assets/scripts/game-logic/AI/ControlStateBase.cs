using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlStateBase 
{
  protected AI _aiRef;

  protected Heuristic _heuristic = new Heuristic();
  public Heuristic HeuristicProperty
  {
    get { return _heuristic; }
  }

  public ControlStateBase(AI ai)
  {    
    _aiRef = ai;
  }

  public virtual void Run()
  {
    CalculateHeuristic();
  }

  List<Int2> _enemyBarracks = new List<Int2>();
  protected void CalculateHeuristic()
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
}

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
