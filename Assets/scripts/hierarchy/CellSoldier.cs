using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSoldier : CellBaseClass 
{ 
  Vector3 _heading = Vector3.zero;
  Vector3 _dir = Vector3.zero;
  Vector3 _target = Vector3.zero;

  float _attackTimer = 0.0f;
  float _magnitude = 1.0f;
  float _gridX = 0.0f, _gridY = 0.0f;

  Int2 _previousPos = Int2.Zero;

  int _enemyId = 0;
  public CellSoldier()
  {    
    Hitpoints = GlobalConstants.CellSoldierHitpoints;
  }

  public override void InitBehaviour()
  {
    // Find enemy ID

    foreach (var b in LevelLoader.Instance.BaseCoordinatesByOwner)
    {
      if (OwnerId != b.Key)
      {
        _enemyId = b.Key;
        break;
      }
    }

    _position = BehaviourRef.transform.position;

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    Coordinates.Set(_gridX, _gridY);
    _previousPos.Set(Coordinates);

    LevelLoader.Instance.Map[Coordinates.X, Coordinates.Y].SoldiersByOwnerHere[OwnerId].Enqueue(this);

    FindTarget();
  }

  Vector3 _position = Vector3.zero;
  public override void Update()
  {
    CheckTarget();

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

    Coordinates.Set(_gridX, _gridY);

    ModelTransform.Rotate(Vector3.right, Time.smoothDeltaTime * 50.0f);
    ModelTransform.Rotate(Vector3.up, Time.smoothDeltaTime * 100.0f);
    ModelTransform.Rotate(Vector3.forward, Time.smoothDeltaTime * 25.0f);

    BehaviourRef.transform.position = _position;
  }

  void CheckTarget()
  {
    if ((int)_target.x == Coordinates.X && (int)_target.y == Coordinates.Y)
    {
      FindTarget();
    }
  }

  bool CanOccupy()
  {
    /*
    if ((Coordinates.X != _previousPos.X
      || Coordinates.Y != _previousPos.Y)
      && LevelLoader.Instance.Map[Coordinates.X, Coordinates.Y].SoldierHere == null)
    {      
      LevelLoader.Instance.Map[_previousPos.X, _previousPos.Y].SoldierHere = null;
      LevelLoader.Instance.Map[Coordinates.X, Coordinates.Y].SoldierHere = this;

      _previousPos.Set(Coordinates);

      return true;
    }
    */

    if (Coordinates.X != _previousPos.X || Coordinates.Y != _previousPos.Y)
    { 
      if (LevelLoader.Instance.Map[_previousPos.X, _previousPos.Y].SoldiersByOwnerHere[OwnerId].Count != 0)
      {
        //Debug.Log("Removing self from " + _previousPos);

        LevelLoader.Instance.Map[_previousPos.X, _previousPos.Y].SoldiersByOwnerHere[OwnerId].Dequeue();
      }

      //Debug.Log("Adding self to " + Coordinates);

      LevelLoader.Instance.Map[Coordinates.X, Coordinates.Y].SoldiersByOwnerHere[OwnerId].Enqueue(this);

      _previousPos.Set(Coordinates);

      return true;
    }

    return false;
  }

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
          // Check soldiers

          if (LevelLoader.Instance.Map[x, y].SoldiersByOwnerHere[_enemyId].Count != 0)            
          {
            
            _enemyPos.Set(LevelLoader.Instance.Map[x, y].SoldiersByOwnerHere[_enemyId].Peek().Coordinates);

            //Debug.Log("Soldier found at " + _enemyPos);

            return true;
          }

          // Check other cells

          if ((LevelLoader.Instance.Map[x, y].CellHere != null && LevelLoader.Instance.Map[x, y].CellHere.OwnerId != OwnerId))
          {
            _enemyPos.Set(x, y);

            //Debug.Log("Enemy cell found at " + _enemyPos);

            return true;
          }

          /*
          if ((LevelLoader.Instance.Map[x, y].SoldierHere != null && LevelLoader.Instance.Map[x, y].SoldierHere.OwnerId != OwnerId)
           || (LevelLoader.Instance.Map[x, y].CellHere != null && LevelLoader.Instance.Map[x, y].CellHere.OwnerId != OwnerId))
          { 
            _enemyPos.Set(x, y);

            //Debug.Log("Enemy found at " + _enemyPos);

            return true;
          }
          */
        }
      }
    }

    return false;
  }

  void FindTarget()
  { 
    float distance = 0.0f;
    float minDistance = float.MaxValue;

    Int2 pos = Int2.Zero;
    Int2 pos2 = Int2.Zero;

    bool found = false;

    foreach (var item in LevelLoader.Instance.BuildingsCoordinatesByOwner[_enemyId])
    {      
      distance = Vector3.Distance(new Vector3(item.X, item.Y), new Vector3(Coordinates.X, Coordinates.Y));

      if (minDistance <= distance)
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

    _target.x = pos.X;
    _target.y = pos.Y;
  }

  Vector3 _enemyPosition3D = Vector3.zero;
  void AttackCell()
  {
    Vector3 posTmp = Vector3.zero;

    if (LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].SoldiersByOwnerHere[_enemyId].Count != 0)
    {
      posTmp = LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].SoldiersByOwnerHere[_enemyId].Peek().BehaviourRef.transform.position;
    }
    else if (LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].CellHere != null)
    {
      posTmp = LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].CellHere.BehaviourRef.transform.position;
    }

    _enemyPosition3D.Set(posTmp.x, posTmp.y, posTmp.z);

    LevelLoader.Instance.SpawnBullet(_position, _enemyPosition3D, _enemyId);

    /*
    if (LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].SoldierHere != null)
    {
      posTmp = LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].SoldierHere.BehaviourRef.transform.position;
    }
    else if (LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].CellHere != null)
    {
      posTmp = LevelLoader.Instance.Map[_enemyPos.X, _enemyPos.Y].CellHere.BehaviourRef.transform.position;
    }
      
    _enemyPosition3D.Set(posTmp.x, posTmp.y, posTmp.z);

    LevelLoader.Instance.SpawnBullet(_position, _enemyPosition3D);
    */

    //Debug.Log("Attacking " + c + " at " + c.Coordinates); 
  }
}
