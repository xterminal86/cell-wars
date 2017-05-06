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

    LevelLoader.Instance.SoldiersMap[Coordinates.X, Coordinates.Y][this.GetHashCode()] = this;

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
      var b = LevelLoader.Instance.Map[item.X, item.Y];

      if (b.CellHere.Type == GlobalConstants.CellType.HOLDER)
      {
        d = Vector3.Distance(WorldCoordinates, b.CellHere.WorldCoordinates);

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
    return (LevelLoader.Instance.Map[(int)_destination.x, (int)_destination.y].CellHere != null);
  }

  bool GridPositionChanged()
  {    
    if (Coordinates.X != _previousPos.X || Coordinates.Y != _previousPos.Y)
    { 
      LevelLoader.Instance.SoldiersMap[_previousPos.X, _previousPos.Y].Remove(this.GetHashCode());
      LevelLoader.Instance.SoldiersMap[Coordinates.X, Coordinates.Y][this.GetHashCode()] = this;

      _previousPos.Set(Coordinates);

      return true;
    }

    return false;
  }

  CellBaseClass _enemy;
  Int2 _enemyPos = Int2.Zero;
  bool FindEnemies()
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
            if (kvp.Value.OwnerId != OwnerId)
            {
              _enemyPos.Set(kvp.Value.Coordinates);
              _enemy = kvp.Value;

              return true;
            }
          }

          // Check other cells second

          if ((LevelLoader.Instance.Map[x, y].CellHere != null             
            && LevelLoader.Instance.Map[x, y].CellHere.OwnerId != OwnerId))
          {
            _enemyPos.Set(x, y);
            _enemy = LevelLoader.Instance.Map[x, y].CellHere;

            //Debug.Log("Enemy cell found at " + _enemyPos);

            return true;
          }
        }
      }
    }

    return false;
  }

  /// <summary>
  /// Finds enemy building to move at.
  /// </summary>
  void FindDestination()
  { 
    float distance = 0.0f;
    float minDistance = float.MaxValue;

    Int2 pos = Int2.Zero;
    Int2 pos2 = Int2.Zero;

    bool found = false;

    foreach (var item in LevelLoader.Instance.BuildingsCoordinatesByOwner[_enemyId])
    {      
      var cell = LevelLoader.Instance.Map[item.X, item.Y].CellHere;

      distance = Vector3.Distance(new Vector3(item.X, item.Y), new Vector3(Coordinates.X, Coordinates.Y));

      if (minDistance <= distance && cell.Type != GlobalConstants.CellType.HOLDER)
      {
        found = true;

        pos.Set(item);
        minDistance = distance;
      }

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
      posTmp = _enemy.WorldCoordinates;
    }
    else if (LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].CellHere != null)
    {
      posTmp = LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].CellHere.BehaviourRef.transform.position;
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
