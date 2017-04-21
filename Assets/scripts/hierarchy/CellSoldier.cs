using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSoldier : CellBaseClass 
{ 
  Vector3 _heading = Vector3.zero;
  Vector3 _dir = Vector3.zero;
  Vector3 _target = new Vector3(14, 14);

  float _attackTimer = 0.0f;
  float _magnitude = 1.0f;
  float _gridX = 0.0f, _gridY = 0.0f;

  Int2 _currentPos = Int2.Zero;
  Int2 _previousPos = Int2.Zero;

  public CellSoldier()
  {    
    Hitpoints = GlobalConstants.CellSoldierHitpoints;
  }

  public override void InitBehaviour()
  {
    _position = BehaviourRef.transform.position;

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    _currentPos.Set(_gridX, _gridY);
    _previousPos.Set(_currentPos);

    LevelLoader.Instance.Map[_currentPos.X, _currentPos.Y].SoldierHere = this;
  }

  Vector3 _position = Vector3.zero;
  public override void Update()
  {
    _position = BehaviourRef.transform.position;

    _heading = (_target - BehaviourRef.transform.position);
    _magnitude = _heading.magnitude;
    _dir = _heading / _magnitude;

    CanOccupy();

    if (!FindEnemies())
    {      
      _position += (_dir * Time.smoothDeltaTime * 0.5f);
    }
    else
    {
      if (_attackTimer >= GlobalConstants.AttackTimeout)
      {
        _attackTimer = 0.0f;
        AttackCell();
      }
    }

    if (_attackTimer < GlobalConstants.AttackTimeout)
    {
      _attackTimer += Time.smoothDeltaTime;
    }

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    _currentPos.Set(_gridX, _gridY);

    BehaviourRef.transform.Rotate(Vector3.right, Time.smoothDeltaTime * 50.0f);
    BehaviourRef.transform.Rotate(Vector3.up, Time.smoothDeltaTime * 100.0f);
    BehaviourRef.transform.Rotate(Vector3.down, Time.smoothDeltaTime * 25.0f);

    BehaviourRef.transform.position = _position;
  }

  bool CanOccupy()
  {
    if ((_currentPos.X != _previousPos.X
      || _currentPos.Y != _previousPos.Y)
      && LevelLoader.Instance.Map[_currentPos.X, _currentPos.Y].SoldierHere == null)
    {      
      LevelLoader.Instance.Map[_previousPos.X, _previousPos.Y].SoldierHere = null;
      LevelLoader.Instance.Map[_currentPos.X, _currentPos.Y].SoldierHere = this;

      _previousPos.Set(_currentPos);

      return true;
    }

    return false;
  }

  Int2 _enemyPos = Int2.Zero;
  bool FindEnemies()
  {
    int lx = _currentPos.X - 1;
    int ly = _currentPos.Y - 1;
    int hx = _currentPos.X + 1;
    int hy = _currentPos.Y + 1;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
         && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          if ((LevelLoader.Instance.Map[x, y].SoldierHere != null && LevelLoader.Instance.Map[x, y].SoldierHere.OwnerId != OwnerId)
           || (LevelLoader.Instance.Map[x, y].CellHere != null && LevelLoader.Instance.Map[x, y].CellHere.OwnerId != OwnerId))
          {           
            _enemyPos.Set(x, y);

            //Debug.Log("Enemy found at " + _enemyPos);

            return true;
          }
        }
      }
    }

    return false;
  }

  void AttackCell()
  {
    var c = LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].CellHere;

    c.ReceiveDamage(1);

    //Debug.Log("Attacking " + c + " at " + c.Coordinates); 
  }
}
