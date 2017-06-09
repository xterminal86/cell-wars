using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FIXME: almost the same as CellSoldier.cs
public class CellHeavy : CellBaseClass
{
  Vector3 _heading = Vector3.zero;
  Vector3 _dir = Vector3.zero;
  Vector3 _destination = Vector3.zero;

  float _moveSpeedModifier = 1.0f;

  float _attackTimer = 0.0f;
  float _magnitude = 1.0f;
  float _gridX = 0.0f, _gridY = 0.0f;

  Int2 _previousPos = Int2.Zero;

  public int SpawnID = -1;
  public CellArsenal BarracksRef;

  public CellHeavy()
  {    
    Type = GlobalConstants.CellType.HEAVY;
    Hitpoints = GlobalConstants.CellHeavyHitpoints;
    Priority = GlobalConstants.CellHeavyPriority;
  }

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 2.0f / 4.0f;
    _animationSpeed = 0.2f / _phaseDuration;
    _zRotationSpeed = 20.0f;

    _position = BehaviourRef.transform.position;

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    Coordinates.Set(_gridX, _gridY);
    _previousPos.Set(Coordinates);

    LevelLoader.Instance.SoldiersMap[Coordinates.X, Coordinates.Y][this.GetHashCode()] = BehaviourRef;

    FindDestination();
  }

  float _resMoveSpeed = 0.0f;
  Vector3 _position = Vector3.zero;
  public override void Update()
  {
    base.Update();

    PlayAnimation();

    if (LevelLoader.Instance.IsGameOver)
    {
      return;
    }

    // Check if destination target is still there
    CheckTargetStatus();

    // Make adjustments if there are holders nearby
    CheckHolders();

    _position = BehaviourRef.transform.position;

    _heading = (_destination - BehaviourRef.transform.position);
    _magnitude = _heading.magnitude;
    _dir = _heading / _magnitude;

    // If this soldier changed its map coordinates, adjust references accordingly
    GridPositionChanged();

    // Try to find enemies nearby
    FindEnemies(GlobalConstants.CellHeavyMinRange, GlobalConstants.CellHeavyMaxRange);

    if (_enemyFound == null)
    { 
      _resMoveSpeed = Time.smoothDeltaTime * (GlobalConstants.HeavyMoveSpeed * _moveSpeedModifier);
      _position += (_dir * _resMoveSpeed);
    }
    else
    {
      if (_attackTimer >= GlobalConstants.HeavyAttackTimeout)
      {
        _attackTimer = 0.0f;

        AttackCell();
      }
    }

    if (_attackTimer < GlobalConstants.HeavyAttackTimeout)
    {
      _attackTimer += Time.smoothDeltaTime;
    }

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    Coordinates.Set(_gridX, _gridY);

    BehaviourRef.transform.position = _position;
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

  Vector3 _enemyPosition3D = Vector3.zero;
  void AttackCell()
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

    LevelLoader.Instance.SpawnSplashBullet(_position, _enemyPosition3D, BehaviourRef, _enemyFound, 
      GlobalConstants.CellHeavyDamage, GlobalConstants.CellHeavySplashRadius);
  }

  public void DelistFromBarracks()
  {
    if (BarracksRef != null)
    {
      BarracksRef.SpawnedSoldiersById[SpawnID] = null;
    }
  }
}
