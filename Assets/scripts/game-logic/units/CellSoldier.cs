using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for assault units
/// </summary>
public class CellSoldier : CellBaseClass 
{ 
  protected Vector3 _heading = Vector3.zero;
  protected Vector3 _dir = Vector3.zero;
  protected Vector3 _destination = Vector3.zero;

  protected float _moveSpeedModifier = 1.0f;

  protected float _attackTimer = 0.0f;
  protected float _magnitude = 1.0f;
  protected float _gridX = 0.0f, _gridY = 0.0f;

  protected Int2 _previousPos = Int2.Zero;

  public int SpawnID = -1;
  public CellBarracks BarracksRef;

  public CellSoldier()
  {    
    IsStationary = false;
  }

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _position = BehaviourRef.transform.position;

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    Coordinates.Set(_gridX, _gridY);
    _previousPos.Set(Coordinates);

    LevelLoader.Instance.SoldiersMap[Coordinates.X, Coordinates.Y][this.GetHashCode()] = BehaviourRef;

    FindDestination();
  }

  Vector3 _position = Vector3.zero;
  public override void Update()
  {
    base.Update();

    // Check if destination target is still there
    CheckTargetStatus();

    // Make adjustments if there are holders nearby
    CheckHolders();

    // If this soldier changed its map coordinates, adjust references accordingly
    GridPositionChanged();
  }

  void CheckHolders()
  {
    float d = 0.0f;

    foreach (var item in LevelLoader.Instance.BuildingsCoordinatesByOwner[_enemyId])
    {
      var obj = LevelLoader.Instance.ObjectsMap[item.X, item.Y];

      /*
      Debug.Log(" Cell : [" + obj + "]");
      Debug.Log(" cell here: [" + obj.CellInstance + "]");
      Debug.Log(" coordinates: [" + item + "]");
      */

      if (obj != null && obj.CellInstance.Type == GlobalConstants.CellType.HOLDER)
      {
        d = Vector3.Distance(WorldCoordinates, obj.CellInstance.WorldCoordinates);

        if (d <= GlobalConstants.CellHolderRange)
        {
          _moveSpeedModifier = GlobalConstants.HolderSlowFactor;
          return;
        }
      }
    }

    _moveSpeedModifier = 1.0f;
  }

  void CheckTargetStatus()
  {
    if (!IsDestinationStillPresent() || ((int)_destination.x == Coordinates.X && (int)_destination.y == Coordinates.Y))
    {
      FindDestination();
    }
  }

  bool IsDestinationStillPresent()
  {
    return (LevelLoader.Instance.ObjectsMap[(int)_destination.x, (int)_destination.y] != null);
  }

  bool GridPositionChanged()
  {    
    if (Coordinates.X != _previousPos.X || Coordinates.Y != _previousPos.Y)
    { 
      LevelLoader.Instance.SoldiersMap[_previousPos.X, _previousPos.Y].Remove(this.GetHashCode());
      LevelLoader.Instance.SoldiersMap[Coordinates.X, Coordinates.Y][this.GetHashCode()] = BehaviourRef;

      _previousPos.Set(Coordinates);

      return true;
    }

    return false;
  }

  /// <summary>
  /// Finds enemy building to move at.
  /// </summary>
  void FindDestination()
  { 
    int distance = 0;
    int minDistance = int.MaxValue;

    Int2 pos = Int2.Zero;
    Int2 pos2 = Int2.Zero;

    bool found = false;

    int priority = 0;

    foreach (var item in LevelLoader.Instance.BuildingsCoordinatesByOwner[_enemyId])
    {      
      var obj = LevelLoader.Instance.ObjectsMap[item.X, item.Y];

      distance = Utils.BlockDistance(item, Coordinates);

      if (obj != null && obj.CellInstance.Type != GlobalConstants.CellType.HOLDER)
      {
        if (obj.CellInstance.Priority > priority && distance < minDistance)
        {
          priority = obj.CellInstance.Priority;

          found = true;

          pos.Set(item);
          minDistance = distance;
        }
      }

      // In case nothing suitable was found, make sure to add at least something
      pos2.Set(item);
    }

    if (!found)
    {
      pos.Set(pos2);
    }

    _destination.x = pos.X;
    _destination.y = pos.Y;
  }

  protected Vector3 _enemyPosition3D = Vector3.zero;
  protected void LockTarget(Callback cb)
  {
    if (_enemyFound != null && _enemyFound.CellInstance == null)
    {
      return;
    }

    _enemyPosition3D.Set(_enemyFound.CellInstance.WorldCoordinates.x, _enemyFound.CellInstance.WorldCoordinates.y, _enemyFound.CellInstance.WorldCoordinates.z);

    if (_enemyFound.CellInstance.Type == GlobalConstants.CellType.DRONE)
    {
      _enemyFound.CellInstance.IsBeingAttacked = true;
    }

    if (cb != null)
      cb();
  }

  public void DelistFromBarracks()
  {
    if (BarracksRef != null)
    {
      BarracksRef.SpawnedSoldiersById[SpawnID] = null;
    }
  }
}
