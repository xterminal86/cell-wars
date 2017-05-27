using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attacker - moves towards nearest enemy building, attacking everything it encounters.
/// </summary>
public class CellSoldier : CellBaseClass 
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
  public CellBarracks BarracksRef;

  public CellSoldier()
  {    
    Type = GlobalConstants.CellType.SOLDIER;
    Hitpoints = GlobalConstants.CellSoldierHitpoints;
    Priority = GlobalConstants.CellSoldierPriority;
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
    if (!FindEnemies())
    {      
      _resMoveSpeed = Time.smoothDeltaTime * (GlobalConstants.AttackerMoveSpeed * _moveSpeedModifier);
      _position += (_dir * _resMoveSpeed);
    }
    else
    {
      if (_attackTimer >= GlobalConstants.SoldierAttackTimeout)
      {
        _attackTimer = 0.0f;

        SearchForPriorityTarget();
        AttackCell();
      }
    }

    if (_attackTimer < GlobalConstants.SoldierAttackTimeout)
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

  CellBehaviour _enemy;
  Int2 _enemyPos = Int2.Zero;
  float _distance = 0.0f;
  bool FindEnemies()
  {
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    int enemyPriority = 0;

    _distance = 0.0f;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
         && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          // Check soldiers first

          foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
          {           
            if (kvp.Value != null && kvp.Value.CellInstance.OwnerId != OwnerId)
            {
              _distance = Vector3.Distance(WorldCoordinates, kvp.Value.CellInstance.WorldCoordinates);

              if (kvp.Value.CellInstance.Priority > enemyPriority && !kvp.Value.IsDestroying && _distance < 1.0f)
              {
                enemyPriority = kvp.Value.CellInstance.Priority;
                _enemyPos.Set(kvp.Value.CellInstance.Coordinates);
                _enemy = kvp.Value;
              }
            }
          }

          // Check other cells second

          if (LevelLoader.Instance.ObjectsMap[x, y] != null
            && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.OwnerId != OwnerId
            && !LevelLoader.Instance.ObjectsMap[x, y].IsDestroying)
          {
            _distance = Vector3.Distance(WorldCoordinates, LevelLoader.Instance.ObjectsMap[x, y].CellInstance.WorldCoordinates);

            if (LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Priority > enemyPriority && _distance < 1.0f)
            {
              enemyPriority = LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Priority;
              _enemyPos.Set(x, y);
              _enemy = LevelLoader.Instance.ObjectsMap[x, y];
            }
          }
        }
      }
    }

    return (enemyPriority != 0);
  }

  // FIXME: lots of duplicate code
  void SearchForPriorityTarget()
  {
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
          && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          // Check soldiers first

          foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
          {           
            if (kvp.Value != null && kvp.Value.CellInstance.OwnerId != OwnerId
              && _enemy != null && kvp.Value.CellInstance.Priority > _enemy.CellInstance.Priority && !kvp.Value.IsDestroying)
            {              
              _distance = Vector3.Distance(WorldCoordinates, kvp.Value.CellInstance.WorldCoordinates);

              if (_distance < 1.0f)
              {
                _enemy = kvp.Value;
                return;
              }
            }
          }

          // Check other cells second

          if (LevelLoader.Instance.ObjectsMap[x, y] != null
            && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.OwnerId != OwnerId
            && !LevelLoader.Instance.ObjectsMap[x, y].IsDestroying
            && _enemy != null && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Priority > _enemy.CellInstance.Priority)
          {            
            _distance = Vector3.Distance(WorldCoordinates, LevelLoader.Instance.ObjectsMap[x, y].CellInstance.WorldCoordinates);

            if (_distance < 1.0f)
            {
              _enemy = LevelLoader.Instance.ObjectsMap[x, y];
              return;
            }
          }
        }
      }
    }
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
    Vector3 posTmp = Vector3.zero;

    if (_enemy != null)
    {
      posTmp = _enemy.CellInstance.WorldCoordinates;
    }
    else if (LevelLoader.Instance.ObjectsMap[_enemyPos.X, _enemyPos.Y] != null)
    {
      posTmp = LevelLoader.Instance.ObjectsMap[_enemyPos.X, _enemyPos.Y].transform.position;
    }

    //Debug.Log("Attacking " + _enemy + " at " + posTmp + " " + _enemy.Coordinates); 

    _enemyPosition3D.Set(posTmp.x, posTmp.y, posTmp.z);

    LevelLoader.Instance.SpawnBullet(_position, _enemyPosition3D, _enemy);
  }

  public void DelistFromBarracks()
  {
    if (BarracksRef != null)
    {
      BarracksRef.SpawnedSoldiersById[SpawnID] = null;
    }
  }
}
